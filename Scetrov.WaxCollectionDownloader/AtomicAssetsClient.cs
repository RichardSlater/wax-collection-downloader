using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Scetrov.WaxCollectionDownloader;

public class AtomicAssetsClient : IAtomicAssetsClient {
    private readonly ILogger<AtomicAssetsClient> _logger;
    private readonly CommandLineOptions _options;
    private readonly string _assetsBaseUri;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonNodeOptions _jsonNodeOptions = default;
    private readonly JsonDocumentOptions _jsonDocumentOptions = default;
    private int _rateLimitRemaining = int.MaxValue;
    private DateTimeOffset _rateLimitResets = DateTimeOffset.UtcNow.AddSeconds(60);
    
    public AtomicAssetsClient(ILogger<AtomicAssetsClient> logger, IOptions<CommandLineOptions> commandLineOptions, IHttpClientFactory httpClientFactory) {
        _logger = logger;
        _options = commandLineOptions.Value;

        var atomicBaseUri = $"https://{_options.AtomicEndpoint}/atomicassets/v1/";

        _assetsBaseUri = $"{atomicBaseUri}assets?collection_name={_options.Collection}&schema_name={_options.Schema}&owner={_options.Account}";
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<JsonNode>> FetchAllAccountPages(CancellationToken stoppingToken = default) {
        var resultIndex = 1;
        var data = new List<JsonNode>();

        while (!stoppingToken.IsCancellationRequested) {
            var page = await FetchAccountPage(resultIndex, stoppingToken);

            if (page.Count == 0) {
                return data.AsEnumerable();
            }

            data.AddRange(page.OfType<JsonNode>());

            resultIndex++;
        }

        return data.AsEnumerable();
    }

    private async Task<JsonArray> FetchAccountPage(int page = 0, CancellationToken stoppingToken = default) {
        var uri = new Uri($"{_assetsBaseUri}&page={page}&limit={_options.PageSize}");
        var client = _httpClientFactory.CreateClient("WaxCollection");
        
        for (var attempts = 0; attempts < _options.Retries; attempts++) {
            var backoff = TimeSpan.FromSeconds((int)Math.Pow(2, attempts));

            if (_rateLimitRemaining <= 1) {
                var delay = (DateTimeOffset.UtcNow - _rateLimitResets) + TimeSpan.FromSeconds(5);
                _logger.LogWarning("Hit the rate limit waiting for {delay} before continuing.", delay);
                await Task.Delay(delay, stoppingToken);
            }
            
            var response = await client.GetAsync(uri, stoppingToken);

            UpdateRateLimiting(response.Headers);

            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStreamAsync(stoppingToken);
                var node = await JsonNode.ParseAsync(content, _jsonNodeOptions, _jsonDocumentOptions, stoppingToken);

                if (node == null) {
                    _logger.LogWarning("HTTP request succeeded, however the payload did not contain a valid JSON document.");
                    break;
                }

                if (node["success"]!.GetValue<bool>()) return node["data"]!.AsArray();
                _logger.LogCritical("HTTP request succeeded, and the payload contained JSON, however the payload indicated a failure.");
                throw new InvalidDataException("One or more requests failed, check above for errors.");
            }

            _logger.LogWarning("Request failed to return a valid payload on Attempt {attempts} of {maxAttempts}", attempts + 1, _options.Retries);
            Thread.Sleep(backoff);
        }

        _logger.LogCritical("Giving up after {maxAttempts}, check the collection, schema and account are valid.", _options.Retries);

        throw new InvalidDataException("One or more requests failed, check above for errors.");
    }

    private void UpdateRateLimiting(HttpResponseHeaders responseHeaders) {
        var rateLimit = GetRateLimitDiscreteValue(responseHeaders, "ratelimit-limit", "x-ratelimit-limit");
        _rateLimitRemaining = GetRateLimitDiscreteValue(responseHeaders, "ratelimit-remaining", "x-ratelimit-remaining");
        _rateLimitResets = GetRateLimitDate(responseHeaders);
        _logger.LogInformation("Rate Limit Remaining: {remaining} of {limit} requests, reset time: {resets}", _rateLimitRemaining, rateLimit, _rateLimitResets);
    }

    private int GetRateLimitDiscreteValue(HttpResponseHeaders responseHeaders, string primary, string alternative) {
        var primaryValue = responseHeaders.GetValues(primary);
        var altValue = responseHeaders.GetValues(alternative);
        var allValues = primaryValue.Concat(altValue).Select(x => int.TryParse(x, out var value) ? value : int.MaxValue).ToArray();
        return allValues.Length != 0 ? allValues.First() : int.MaxValue;
    }
    
    private DateTimeOffset GetRateLimitDate(HttpResponseHeaders responseHeaders) {
        var primaryValue = responseHeaders.GetValues("ratelimit-reset").ToArray();
        var altValue = responseHeaders.GetValues("x-ratelimit-reset").ToArray();

        if (primaryValue.Length != 0) {
            return int.TryParse(primaryValue.First(), out var value) ? DateTimeOffset.UtcNow.AddSeconds(value) : DateTimeOffset.UtcNow;
        }
        
        if (altValue.Length != 0) {
            return long.TryParse(primaryValue.First(), out var value) ? DateTimeOffset.FromUnixTimeSeconds(value) : DateTimeOffset.UtcNow;
        }

        return DateTimeOffset.UtcNow;
    }
}
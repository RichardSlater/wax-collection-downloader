using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Scetrov.WaxCollectionDownloader;

public class Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime, IOptions<CommandLineOptions> commandLineOptions, IAtomicAssetsClient atomicAssetsClient, IExporter exporter) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        var options = commandLineOptions.Value;
        logger.LogInformation("Started Fetching Data with Parameters [Account: {account}, Collection: {collection}, Schema: {schema}] using endpoint: '{waxRpcEndpoint}'", options.Account, options.Collection, options.Schema, options.AtomicEndpoint);
        
        // data fetching phase
        var fetchStopwatch = Stopwatch.StartNew();
        var results = await atomicAssetsClient.FetchAllAccountPages(stoppingToken);
        var nodes = results as JsonNode[] ?? results.ToArray();
        fetchStopwatch.Stop();
        logger.LogInformation("{account} has {resultCount} assets in the '{collection}' collection, matching the schema '{schema}'.", options.Account, nodes.Count(), options.Collection, options.Schema);
        logger.LogInformation("Fetching Data completed after {sw}", fetchStopwatch.Elapsed);
        
        // output phase
        var outputStopwatch = Stopwatch.StartNew();
        var outputFile = exporter.WriteFile(nodes);
        outputStopwatch.Stop();
        logger.LogInformation("Successfully wrote {outputFile}", outputFile);
        logger.LogInformation("Writing output completed after {sw}", outputStopwatch.Elapsed);
        
        hostApplicationLifetime.StopApplication();
    }
}
using System.Dynamic;
using System.Globalization;
using System.Text.Json.Nodes;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Scetrov.WaxCollectionDownloader;

public class CommaSeparatedValueExporter(ILogger<CommaSeparatedValueExporter> logger, IOptions<CommandLineOptions> commandLineOptions) : IExporter {
    private readonly CommandLineOptions _options = commandLineOptions.Value;

    public string WriteFile(IEnumerable<JsonNode> nodes) {
        if (_options.OutputFile == null) {
            throw new InvalidOperationException("OutputFile was null... awkward.");
        }

        var fullyQualifiedFilename = Path.GetFullPath(_options.OutputFile);

        var jsonNodes = nodes as JsonNode[] ?? nodes.ToArray();
        var headers = GetHeaders(jsonNodes);
        
        using var writer = new StreamWriter(fullyQualifiedFilename);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        foreach (var header in headers) {
            logger.LogInformation("Writing header: {key}", header.Key);
            csv.WriteField(header.Key);
        }
        
        csv.NextRecord();

        logger.LogInformation("Writing {count} records", jsonNodes.Length);
        foreach (var node in jsonNodes) {
            foreach (var header in headers) {
                try {
                    var value = header.Value(node);
                    csv.WriteField(value);
                }
                catch {
                    csv.WriteField("-");
                }
            }
            
            csv.NextRecord();
        }

        logger.LogInformation("Finished writing records.");
        return fullyQualifiedFilename;
    }

    public Dictionary<string, Func<JsonNode, string>> GetHeaders(IEnumerable<JsonNode> nodes) {
        var headers = new Dictionary<string, Func<JsonNode, string>> {
            { "asset_id", n => n["asset_id"]!.GetValue<string>() },
            { "owner", n => n["owner"]!.GetValue<string>() },
            { "is_transferable", n => n["is_transferable"]!.GetValue<bool>() ? "\u2705" : "\u274c" },
            { "is_burnable", n => n["is_burnable"]!.GetValue<bool>() ? "\u2705" : "\u274c" },
            { "collection", n => n["collection"]!["name"]!.GetValue<string>() },
            { "schema", n => n["schema"]!["schema_name"]!.GetValue<string>() },
            { "template_id", n => n["template"]!["template_id"]!.GetValue<string>() },
            { "max_supply", n => {
                    var value = n["template"]!["max_supply"]!.GetValue<string>(); 
                    return value == "0" ? "\u221e" : value;
                }
            },
            { "issued_supply", n => n["template"]!["issued_supply"]!.GetValue<string>() },
            { "mint", n => n["template_mint"]!.GetValue<string>() },
            { "updated_at_block", n => n["updated_at_block"]!.GetValue<string>() },
            { "updated_at_time", n => ExtractDateTime(n, "updated_at_time")},
            { "transferred_at_block", n => n["transferred_at_block"]!.GetValue<string>() },
            { "transferred_at_time", n => ExtractDateTime(n, "transferred_at_time")},
            { "minted_at_block", n => n["minted_at_block"]!.GetValue<string>() },
            { "minted_at_time", n =>  ExtractDateTime(n, "minted_at_time")},
            { "name", n => n["name"]!.GetValue<string>() }
        };

        foreach (var node in nodes) {
            foreach (var attribute in  node["data"]!.AsObject()) {
                if (headers.ContainsKey(attribute.Key)) continue;
                headers.Add(attribute.Key, n => n["data"]![attribute.Key]!.GetValue<string>());
            }
        }

        return headers;
    }

    public string ExtractDateTime(JsonNode n, string attribute) {
        var value = n[attribute]!.GetValue<string>();
        return long.TryParse(value, out var longValue) ? DateTimeOffset.FromUnixTimeMilliseconds(longValue).ToString() : "-";
    }
}
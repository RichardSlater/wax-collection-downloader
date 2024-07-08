using System.Text.Json.Nodes;

namespace Scetrov.WaxCollectionDownloader;

public interface IExporter {
    string WriteFile(IEnumerable<JsonNode> nodes);
}
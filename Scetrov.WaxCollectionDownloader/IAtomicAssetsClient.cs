using System.Text.Json.Nodes;

namespace Scetrov.WaxCollectionDownloader;

public interface IAtomicAssetsClient {
    Task<IEnumerable<JsonNode>> FetchAllAccountPages(CancellationToken stoppingToken = default);
}
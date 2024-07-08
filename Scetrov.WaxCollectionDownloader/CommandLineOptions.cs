using CommandLine;

namespace Scetrov.WaxCollectionDownloader;

public class CommandLineOptions {
    [Option('c', "collection", Required = true, HelpText = "WAX name of the collection (case sensitive).")]
    public string? Collection { get; set; }

    [Option('s', "schema", Required = true, HelpText = "WAX name of the schema (case sensitive).")]
    public string? Schema { get; set; }
    
    [Option('a', "account", Required = true, HelpText = "WAX name of the account (case sensitive).")]
    public string? Account { get; set; }
    
    [Option('o', "output", Required = true, HelpText = "CSV output file.")]
    public string? OutputFile { get; set; }
    
    [Option('e', "endpoint", Required = false, HelpText = "Atomic Assets API Endpoint.", Default = "atomic-wax.tacocrypto.io")]
    public string? AtomicEndpoint { get; set; }
    
    [Option('p', "pageSize", Required = false, HelpText = "Size of each API page.", Default = 1000)]
    public int PageSize { get; set; }
    
    [Option('r', "retries", Required = false, HelpText = "Number of retries before giving up.", Default = 10)]
    public int Retries { get; set; }
}
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scetrov.WaxCollectionDownloader;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services => {
    services.AddScoped<IAtomicAssetsClient, AtomicAssetsClient>();
    services.AddScoped<IExporter, CommaSeparatedValueExporter>();
    services.AddHttpClient();
    services.AddHostedService<Worker>();
    // https://siderite.dev/blog/using-commandlineparser-in-way-friendly-to-depende
    services.AddOptions<CommandLineOptions>()
        .Configure(opt => {
            var result = Parser.Default.ParseArguments(() => opt, Environment.GetCommandLineArgs());
            result.WithNotParsed(errors => Environment.Exit(9));
        });
});

await builder.Build().RunAsync();
using Worms.Hub.ReplayProcessor;

var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables("WORMS_")
    .Build();

// Run as a batch job when in Production
// These means we can scale to zero when there are no messages to process
if (configuration["BATCH"] == "true")
{
    var serviceProvider = new ServiceCollection().AddReplayProcessorServices()
        .AddLogging(builder => builder.AddConsole())
        .AddSingleton<IConfiguration>(configuration)
        .BuildServiceProvider();
    var processor = serviceProvider.GetService<Processor>();
    await processor!.ProcessReplay().ConfigureAwait(false);
    return;
}

// Run as a hosted service when in Development
// This means we can run continuously and check for messages
var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((_, builder) => builder.AddConsole())
    .ConfigureAppConfiguration(config => config.AddConfiguration(configuration))
    .ConfigureServices(s => s.AddReplayProcessorServices())
    .ConfigureServices(services => services.AddHostedService<CheckForMessagesService>())
    .Build();

await host.RunAsync().ConfigureAwait(false);

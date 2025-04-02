using Worms.Hub.ReplayUpdater;

var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables("WORMS_")
    .Build();

// Run as a batch job when in Production
// These means we can scale to zero when there are no messages to process
if (configuration["BATCH"] == "true")
{
    var serviceProvider = new ServiceCollection().AddReplayUpdaterServices()
        .AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole(c => c.SingleLine = true))
        .AddSingleton<IConfiguration>(configuration)
        .BuildServiceProvider();
    var processor = serviceProvider.GetService<Processor>();
    await processor!.UpdateReplay();
    return;
}

// Run as a hosted service when in Development
// This means we can run continuously and check for messages
var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((_, builder) => builder.AddSimpleConsole(c => c.SingleLine = true))
    .ConfigureAppConfiguration(config => config.AddConfiguration(configuration))
    .ConfigureServices(s => s.AddReplayUpdaterServices())
    .ConfigureServices(services => services.AddHostedService<CheckForMessagesService>())
    .Build();

await host.RunAsync();

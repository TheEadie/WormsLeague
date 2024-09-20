using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Hub.ReplayProcessor;
using Worms.Hub.Storage;

var configuration = new ConfigurationBuilder().AddEnvironmentVariables("WORMS_").Build();
var serviceProvider = new ServiceCollection().AddReplayProcessorServices()
    .AddHubStorageServices()
    .AddWormsArmageddonGameServices()
    .AddLogging(builder => builder.AddConsole())
    .AddSingleton<IConfiguration>(configuration)
    .BuildServiceProvider();

var processor = serviceProvider.GetService<Processor>();

await processor!.ProcessReplay().ConfigureAwait(false);

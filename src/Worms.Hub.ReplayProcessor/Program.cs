using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Hub.ReplayProcessor;

var configuration = new ConfigurationBuilder().AddEnvironmentVariables("WORMS_").Build();
var serviceProvider = new ServiceCollection().AddReplayProcessorServices()
    .AddWormsArmageddonGameServices()
    .AddLogging(builder => builder.AddConsole())
    .AddSingleton<IConfiguration>(configuration)
    .BuildServiceProvider();

var processor = serviceProvider.GetService<Processor>();

await processor!.ProcessReplay(args[0]).ConfigureAwait(false);

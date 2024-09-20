using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game;
using Worms.Hub.ReplayProcessor;

var serviceProvider = new ServiceCollection().AddReplayProcessorServices()
    .AddWormsArmageddonGameServices()
    .BuildServiceProvider();

var processor = serviceProvider.GetService<Processor>();

await processor!.ProcessReplay(args[0]).ConfigureAwait(false);

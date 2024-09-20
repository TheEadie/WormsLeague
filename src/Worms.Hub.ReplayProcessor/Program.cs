using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game;
using Worms.Hub.ReplayProcessor;

var serviceCollection = new ServiceCollection().AddWormsArmageddonGameServices();
var serviceProvider = serviceCollection.BuildServiceProvider();

var processor = serviceProvider.GetService<Processor>();

await processor!.ProcessReplay(args[0]).ConfigureAwait(false);

using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Replays;

Console.WriteLine("Starting replay processor...");

var serviceCollection = new ServiceCollection().AddWormsArmageddonGameServices();
var serviceProvider = serviceCollection.BuildServiceProvider();

var gameLocator = serviceProvider.GetService<IWormsLocator>();
var logGenerator = serviceProvider.GetService<IReplayLogGenerator>();

// Get message from queue
// Grab the replay path from the message
var replayPath = args[0];
var userHomeDirectory = Environment.GetEnvironmentVariable("HOME");
// Generate the replay log and save it
var gameInfo = gameLocator!.Find();

if (gameInfo.IsInstalled)
{
    Console.WriteLine("Game found at: {0}", gameInfo.ExeLocation);
}
else
{
    Console.WriteLine("Looking in {0}", userHomeDirectory);
    Console.WriteLine("Game not found. Please install the game and try again.");
    return;
}

await logGenerator!.GenerateReplayLog(replayPath).ConfigureAwait(false);

Console.WriteLine("Replay processor finished.");

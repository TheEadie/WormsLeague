using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Replays;

namespace Worms.Hub.ReplayProcessor;

public class Processor(
    IWormsLocator wormsLocator,
    IReplayLogGenerator logGenerator,
    IReplayFrameExtractor replayFrameExtractor)
{
    public async Task ProcessReplay(string replayPath)
    {
        Console.WriteLine("Starting replay processor...");

        // TODO Get message from queue

        var userHomeDirectory = Environment.GetEnvironmentVariable("HOME");
        var gameInfo = wormsLocator.Find();

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

        await replayFrameExtractor.ExtractReplayFrames(replayPath, 5, TimeSpan.Zero, TimeSpan.FromSeconds(60))
            .ConfigureAwait(false);
        await logGenerator.GenerateReplayLog(replayPath).ConfigureAwait(false);

        Console.WriteLine("Replay processor finished.");
    }
}

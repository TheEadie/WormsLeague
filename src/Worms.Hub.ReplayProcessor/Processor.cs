using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Replays;

namespace Worms.Hub.ReplayProcessor;

public class Processor(
    IWormsLocator wormsLocator,
    IReplayLogGenerator logGenerator,
    IReplayFrameExtractor replayFrameExtractor,
    ILogger<Processor> logger)
{
    public async Task ProcessReplay(string replayPath)
    {
        logger.LogInformation("Starting replay processor...");

        // TODO Get message from queue

        if (!GameIsInstalled())
        {
            return;
        }

        await replayFrameExtractor.ExtractReplayFrames(replayPath, 5, TimeSpan.Zero, TimeSpan.FromSeconds(60))
            .ConfigureAwait(false);
        await logGenerator.GenerateReplayLog(replayPath).ConfigureAwait(false);

        // TODO Update Database and File share

        logger.LogInformation("Replay processor finished.");
    }

    private bool GameIsInstalled()
    {
        var userHomeDirectory = Environment.GetEnvironmentVariable("HOME");
        var gameInfo = wormsLocator.Find();

        if (gameInfo.IsInstalled)
        {
            logger.LogDebug("Game found at: {Path}", gameInfo.ExeLocation);
        }
        else
        {
            logger.LogInformation("Looking in {Directory}", userHomeDirectory);
            logger.LogError("Game not found. Please install the game and try again.");
            return false;
        }

        return true;
    }
}

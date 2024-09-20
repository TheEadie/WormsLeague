using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Replays;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.ReplayProcessor;

public class Processor(
    IWormsLocator wormsLocator,
    IReplayLogGenerator logGenerator,
    IRepository<Replay> replayRepository,
    ReplayFiles replayFiles,
    ILogger<Processor> logger)
{
    public async Task ProcessReplay()
    {
        logger.LogInformation("Starting replay processor...");

        // TODO Get replay ID from queue
        const string id = "1";

        // Check replay is in database
        var replay = replayRepository.GetAll().FirstOrDefault(x => x.Id == id);
        if (replay is null)
        {
            logger.LogError("Replay not found in database: {Id}", id);
            return;
        }

        var replayPath = replayFiles.GetReplayPath(replay);

        // Check game is installed
        if (!GameIsInstalled())
        {
            return;
        }

        // Generate replay log
        await logGenerator.GenerateReplayLog(replayPath).ConfigureAwait(false);
        var logPath = replayFiles.GetLogPath(replay);

        if (logPath is null)
        {
            logger.LogError("Log file not found from replay path: {ReplayPath}", replayPath);
            return;
        }

        var replayLog = await File.ReadAllTextAsync(logPath).ConfigureAwait(false);

        // Update the database with the log
        var updatedReplay = replay with
        {
            Status = "Processed",
            FullLog = replayLog
        };
        replayRepository.Update(updatedReplay);

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

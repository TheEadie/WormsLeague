using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Replays;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.ReplayProcessor;

public class Processor(
    IWormsLocator wormsLocator,
    IReplayLogGenerator logGenerator,
    IRepository<Replay> replayRepository,
    ILogger<Processor> logger)
{
    public async Task ProcessReplay(string replayPath)
    {
        logger.LogInformation("Starting replay processor...");

        // TODO Get replay ID from queue

        // Check replay is in database
        var replay = replayRepository.GetAll().FirstOrDefault(x => x.Filename == replayPath);
        if (replay is null)
        {
            logger.LogError("Replay not found in database: {ReplayPath}", replayPath);
            return;
        }

        // Check game is installed
        if (!GameIsInstalled())
        {
            return;
        }

        // Generate replay log
        await logGenerator.GenerateReplayLog(replayPath).ConfigureAwait(false);
        var logPath = GetLogPath(replayPath);

        if (logPath is null)
        {
            logger.LogError("Log file not found from replay path: {ReplayPath}", replayPath);
            return;
        }

        var replayLog = await File.ReadAllTextAsync(logPath).ConfigureAwait(false);

        // Update the database with the log
        var updatedReplay = replay with { FullLog = replayLog };
        replayRepository.Update(updatedReplay);

        logger.LogInformation("Replay processor finished.");
    }

    private static string? GetLogPath(string waGamePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(waGamePath);
        var folder = Path.GetDirectoryName(waGamePath);

        if (folder is null)
        {
            return null;
        }

        var logPath = Path.Combine(folder, fileName + ".log");
        return File.Exists(logPath) ? logPath : null;
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

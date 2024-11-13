using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Replays;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;
using Worms.Hub.Storage.Files;
using Worms.Hub.Storage.Queues;

namespace Worms.Hub.ReplayProcessor;

internal sealed class Processor(
    IMessageQueue<ReplayToProcessMessage> messageQueue,
    IWormsLocator wormsLocator,
    IReplayLogGenerator logGenerator,
    IRepository<Replay> replayRepository,
    ReplayFiles replayFiles,
    ILogger<Processor> logger)
{
    public async Task ProcessReplay()
    {
        logger.LogInformation("Starting replay processor...");

        var (message, token) = await messageQueue.DequeueMessage().ConfigureAwait(false);
        if (message is null || token is null)
        {
            logger.LogInformation("No messages to process.");
            return;
        }

        // Check replay is in database
        var replay = replayRepository.GetAll().FirstOrDefault(x => x.Id == message.ReplayId);
        if (replay is null)
        {
            logger.LogError("Replay not found in database: {Id}", message.ReplayId);
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

        // Delete the message from the queue
        await messageQueue.DeleteMessage(token).ConfigureAwait(false);

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

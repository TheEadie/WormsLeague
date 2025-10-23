using Worms.Armageddon.Files.Replays.Text;
using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Queues;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;
using Worms.Hub.Storage.Files;

namespace Worms.Hub.Gateway.Worker;

internal sealed class Processor(
    IMessageQueue<ReplayToUpdateMessage> messageQueue,
    IRepository<Replay> replayRepository,
    ReplayFiles replayFiles,
    IAnnouncer announcer,
    IReplayTextReader replayTextReader,
    ILogger<Processor> logger)
{
    public async Task UpdateReplay()
    {
        logger.LogInformation("Starting replay updater...");

        var (message, token) = await messageQueue.DequeueMessage();
        if (message is null || token is null)
        {
            logger.LogInformation("No messages to process.");
            return;
        }

        // Check replay is in the folder
        var replayPath = replayFiles.GetReplayPath(message.ReplayFileName);
        if (!File.Exists(replayPath))
        {
            logger.LogError("Replay not found on disk: {ReplayPath}", replayPath);
            return;
        }

        // Check replay has a log file generated
        var logPath = replayFiles.GetLogPath(message.ReplayFileName);
        if (!File.Exists(logPath))
        {
            logger.LogError("Log file not found: {LogPath}", logPath);
            return;
        }

        // Check the replay exists in the database
        var replay = replayRepository.GetAll().FirstOrDefault(r => r.Filename == message.ReplayFileName);
        if (replay is null)
        {
            logger.LogError("Replay not found in database: {ReplayFileName}", message.ReplayFileName);
            return;
        }

        var replayLog = await File.ReadAllTextAsync(logPath);

        // Update the database with the log
        var updatedReplay = replay with
        {
            Status = "Processed",
            FullLog = replayLog
        };
        replayRepository.Update(updatedReplay);

        // Parse the replay log
        var replayModel = replayTextReader.GetModel(replayLog);

        // Announce game complete
        await announcer.AnnounceGameComplete(replayModel.Winner);

        // Delete the message from the queue
        await messageQueue.DeleteMessage(token);

        logger.LogInformation("Replay updater finished.");
    }
}

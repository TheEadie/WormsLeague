using Worms.Armageddon.Game;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;
using Worms.Hub.Storage.Files;
using Worms.Hub.Storage.Queues;

namespace Worms.Hub.ReplayProcessor;

internal sealed class Processor(
    IMessageQueue<ReplayToProcessMessage> messageQueue,
    IWormsArmageddon wormsArmageddon,
    IRepository<Replay> replayRepository,
    ReplayFiles replayFiles,
    ILogger<Processor> logger)
{
    public async Task ProcessReplay()
    {
        logger.LogInformation("Starting replay processor...");

        var (message, token) = await messageQueue.DequeueMessage();
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
            const string copyLocation = "/game";
            const string installLocation = "/root/.wine/drive_c/WA";

            logger.LogInformation($"Game not found. Checking {copyLocation} directory...");
            if (Directory.Exists(copyLocation))
            {
                logger.LogInformation($"Game files found in {copyLocation} directory. Moving to {installLocation}");
                CopyDirectory(copyLocation, installLocation, true);
                logger.LogInformation("Game files moved successfully.");
            }
            else
            {
                logger.LogError($"Game not installed and Game files not found in {copyLocation} directory.");
                return;
            }

            // Check game is installed again
            if (!GameIsInstalled())
            {
                logger.LogError("Game not found. Please install the game and try again.");
                return;
            }
        }

        // Generate replay log
        await wormsArmageddon.GenerateReplayLog(replayPath);
        var logPath = replayFiles.GetLogPath(replay);

        if (logPath is null)
        {
            logger.LogError("Log file not found from replay path: {ReplayPath}", replayPath);
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

        // Delete the message from the queue
        await messageQueue.DeleteMessage(token);

        logger.LogInformation("Replay processor finished.");
    }

    private bool GameIsInstalled()
    {
        var userHomeDirectory = Environment.GetEnvironmentVariable("HOME");
        var gameInfo = wormsArmageddon.FindInstallation();

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

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        }

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
        _ = Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            _ = file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (var subDir in dirs)
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}

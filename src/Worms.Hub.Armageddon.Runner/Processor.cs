using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Armageddon.Game;
using Worms.Armageddon.Gifs;
using Worms.Hub.Queues;

namespace Worms.Hub.Armageddon.Runner;

internal sealed class Processor(
    IMessageQueue<ReplayToProcessMessage> inputQueue,
    IMessageQueue<ReplayToUpdateMessage> outputQueue,
    IWormsArmageddon wormsArmageddon,
    IReplayTextReader replayTextReader,
    GifCreator gifCreator,
    IConfiguration configuration,
    ILogger<Processor> logger)
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    public async Task ProcessReplay()
    {
        logger.LogInformation("Starting replay processor...");

        var (message, token, activityContext) = await inputQueue.DequeueMessage();

        using var span = Telemetry.Source.StartActivity(
            "WA Runner - Process Replay",
            ActivityKind.Consumer,
            activityContext);

        if (message is null || token is null)
        {
            logger.LogInformation("No messages to process.");
            return;
        }

        var replayPath = GetReplayPath(message.ReplayFileName);

        // Check replay is in the folder
        if (!File.Exists(replayPath))
        {
            logger.LogError("Replay not found on disk: {ReplayPath}", replayPath);
            return;
        }

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
        var logPath = GetLogPath(replayPath);

        if (logPath is null)
        {
            logger.LogError("Log file not found from replay path: {ReplayPath}", replayPath);
            return;
        }

        // Parse the log to get turn timings
        var logText = await File.ReadAllTextAsync(logPath);
        var replayResource = replayTextReader.GetModel(logText);

        // Find the turn with the most total damage
        var turnGifs = new List<TurnGif>();
        var replayFolder = GetReplayFolderPath();
        var turns = replayResource.Turns.ToList();
        var bestTurn = turns
            .Select((turn, index) => (Turn: turn, Index: index))
            .MaxBy(x => x.Turn.Damage.Sum(d => d.HealthLost));

        if (bestTurn != default)
        {
            var turnNumber = bestTurn.Index + 1;
            var totalDamage = bestTurn.Turn.Damage.Sum(d => d.HealthLost);
            logger.LogInformation(
                "Selected turn {TurnNumber} for GIF generation ({Damage} total damage)",
                turnNumber,
                totalDamage);

            try
            {
                var gifFileName = await gifCreator.CreateGif(
                    replayPath,
                    bestTurn.Turn.Start,
                    bestTurn.Turn.End,
                    turnNumber,
                    replayFolder);

                turnGifs.Add(new TurnGif(turnNumber, gifFileName));
                logger.LogInformation("Generated GIF for turn {TurnNumber}: {GifFileName}", turnNumber, gifFileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate GIF for turn {TurnNumber}", turnNumber);
            }
        }

        // Send a message to the replay updater queue
        await outputQueue.EnqueueMessage(new ReplayToUpdateMessage(message.ReplayFileName, turnGifs));

        // Delete the message from the queue
        await inputQueue.DeleteMessage(token);

        logger.LogInformation("Replay processor finished. Generated {GifCount} GIFs.", turnGifs.Count);
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

    private string GetReplayPath(string replayFileName) => Path.Combine(GetReplayFolderPath(), replayFileName);

    private string GetReplayFolderPath() =>
        configuration["Storage:TempReplayFolder"] ?? throw new ArgumentException("Temp replay folder not configured");

    private static string? GetLogPath(string replayPath)
    {
        var folder = Path.GetDirectoryName(replayPath) ?? throw new ArgumentException("Replay path is invalid");
        var fileName = replayPath.EndsWith(".WAGame", StringComparison.InvariantCultureIgnoreCase)
            ? Path.GetFileNameWithoutExtension(replayPath)
            : Path.GetFileName(replayPath);

        var logPath = Path.Combine(folder, $"{fileName}.log");
        return File.Exists(logPath) ? logPath : null;
    }
}

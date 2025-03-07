using Microsoft.Extensions.Configuration;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Files;

public class ReplayFiles(IConfiguration configuration)
{
    public string GetReplayPath(Replay replay)
    {
        ArgumentNullException.ThrowIfNull(replay);
        return Path.Combine(GetReplayFolderPath(), replay.Filename);
    }

    public string? GetLogPath(Replay replay)
    {
        ArgumentNullException.ThrowIfNull(replay);

        var fileName = replay.Filename.EndsWith(".WAGame", StringComparison.InvariantCultureIgnoreCase)
            ? Path.GetFileNameWithoutExtension(replay.Filename)
            : replay.Filename;

        var logPath = Path.Combine(GetReplayFolderPath(), $"{fileName}.log");
        return File.Exists(logPath) ? logPath : null;
    }

    public async Task<string> SaveFileContents(Stream fileContentsStream)
    {
        ArgumentNullException.ThrowIfNull(fileContentsStream);

        var generatedFileName = Path.GetRandomFileName();
        var tempReplayFolderPath = GetReplayFolderPath();

        if (!Path.Exists(tempReplayFolderPath))
        {
            _ = Directory.CreateDirectory(tempReplayFolderPath);
        }

        var saveFilePath = Path.Combine(tempReplayFolderPath, generatedFileName);

        var fileStream = File.Create(saveFilePath);
        await using var stream = fileStream;
        await fileContentsStream.CopyToAsync(fileStream);
        await fileStream.DisposeAsync();
        return generatedFileName;
    }

    private string GetReplayFolderPath() =>
        configuration["Storage:TempReplayFolder"] ?? throw new ArgumentException("Temp replay folder not configured");
}

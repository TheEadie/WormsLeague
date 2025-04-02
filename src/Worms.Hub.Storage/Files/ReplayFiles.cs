using Microsoft.Extensions.Configuration;

namespace Worms.Hub.Storage.Files;

public class ReplayFiles(IConfiguration configuration)
{
    public string GetReplayPath(string replayFileName)
    {
        ArgumentNullException.ThrowIfNull(replayFileName);
        return Path.Combine(GetReplayFolderPath(), replayFileName);
    }

    public string? GetLogPath(string replayFileName)
    {
        ArgumentNullException.ThrowIfNull(replayFileName);
        var fileName = replayFileName.EndsWith(".WAGame", StringComparison.InvariantCultureIgnoreCase)
            ? Path.GetFileNameWithoutExtension(replayFileName)
            : replayFileName;

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

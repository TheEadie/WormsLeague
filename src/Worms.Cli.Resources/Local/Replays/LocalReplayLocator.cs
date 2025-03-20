using System.IO.Abstractions;
using Worms.Armageddon.Game;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayLocator(IWormsArmageddon wormsArmageddon, IFileSystem fileSystem) : ILocalReplayLocator
{
    public IReadOnlyCollection<ReplayPaths> GetReplayPaths(string searchPattern)
    {
        var gameInfo = wormsArmageddon.FindInstallation();

        if (!gameInfo.IsInstalled)
        {
            return [];
        }

        var waGamePaths = fileSystem.Directory.GetFiles(gameInfo.ReplayFolder, $"{searchPattern}*.WAgame");
        return [.. waGamePaths.Select(x => new ReplayPaths(x, GetLogPath(x)))];
    }

    private string? GetLogPath(string waGamePath)
    {
        var fileName = fileSystem.Path.GetFileNameWithoutExtension(waGamePath);
        var folder = fileSystem.Path.GetDirectoryName(waGamePath);

        if (folder is null)
        {
            return null;
        }

        var logPath = fileSystem.Path.Combine(folder, fileName + ".log");
        return fileSystem.File.Exists(logPath) ? logPath : null;
    }
}

using System.IO.Abstractions;
using Worms.Armageddon.Game;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayLocator : ILocalReplayLocator
{
    private readonly IWormsLocator _wormsLocator;
    private readonly IFileSystem _fileSystem;

    public LocalReplayLocator(IWormsLocator wormsLocator, IFileSystem fileSystem)
    {
        _wormsLocator = wormsLocator;
        _fileSystem = fileSystem;
    }

    public IReadOnlyCollection<ReplayPaths> GetReplayPaths(string pattern)
    {
        var gameInfo = _wormsLocator.Find();

        if (!gameInfo.IsInstalled)
        {
            return new List<ReplayPaths>(0);
        }

        var waGamePaths = _fileSystem.Directory.GetFiles(gameInfo.ReplayFolder, $"{pattern}*.WAgame");
        return waGamePaths.Select(x => new ReplayPaths(x, GetLogPath(x))).ToList();
    }

    private string? GetLogPath(string waGamePath)
    {
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(waGamePath);
        var folder = _fileSystem.Path.GetDirectoryName(waGamePath);

        if (folder is null)
        {
            return null;
        }

        var logPath = _fileSystem.Path.Combine(folder, fileName + ".log");
        return _fileSystem.File.Exists(logPath) ? logPath : null;
    }
}

using System.Globalization;
using System.IO.Abstractions.TestingHelpers;

namespace Worms.Armageddon.Game.Fake;

internal sealed class Installed : IWormsArmageddon
{
    private readonly string _path;
    private readonly Version _version;
    private readonly MockFileSystem _fileSystem;
    private readonly bool _hostCreatesReplay;

    public Installed(
        MockFileSystem fileSystem,
        string? path = null,
        Version? version = null,
        bool hostCreatesReplay = true)
    {
        _fileSystem = fileSystem;
        _hostCreatesReplay = hostCreatesReplay;
        _path = path ?? @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon";
        _version = version ?? new Version(1, 0, 0, 0);

        _fileSystem.AddDirectory(_path);
        _fileSystem.AddDirectory(Path.Combine(_path, "User"));
        _fileSystem.AddDirectory(Path.Combine(_path, "User", "Schemes"));
        _fileSystem.AddDirectory(Path.Combine(_path, "User", "Games"));
        _fileSystem.AddDirectory(Path.Combine(_path, "User", "Capture"));
        _fileSystem.AddFile(Path.Combine(_path, "WA.exe"), new MockFileData([]));
    }

    public GameInfo FindInstallation()
    {
        return new GameInfo(
            true,
            Path.Combine(_path, "WA.exe"),
            "WA",
            _version,
            Path.Combine(_path, "User", "Schemes"),
            Path.Combine(_path, "User", "Games"),
            Path.Combine(_path, "User", "Capture"));
    }

    public Task Host()
    {
        if (_hostCreatesReplay)
        {
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
            _fileSystem.AddEmptyFile(Path.Combine(_path, "User", "Games", $"{dateTime} [Offline] 1-UP, 2-UP.WAGame"));
        }

        return Task.CompletedTask;
    }

    public Task GenerateReplayLog(string replayPath)
    {
        if (_fileSystem.File.Exists(replayPath))
        {
            _fileSystem.AddEmptyFile(replayPath.Replace(".WAGame", ".log", StringComparison.InvariantCulture));
        }

        return Task.CompletedTask;
    }

    public Task PlayReplay(string replayPath) => Task.CompletedTask;

    public Task PlayReplay(string replayPath, TimeSpan startTime) => Task.CompletedTask;

    public Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480)
    {
        if (_fileSystem.File.Exists(replayPath))
        {
            var frames = (int) ((endTime - startTime).TotalSeconds * fps);
            var replayFileName = _fileSystem.Path.GetFileNameWithoutExtension(replayPath);
            for (var i = 0; i < frames; i++)
            {
                _fileSystem.AddEmptyFile(Path.Combine(_path, "User", "Capture", $"{replayFileName}_{i,4}.png"));
            }
        }

        return Task.CompletedTask;
    }
}

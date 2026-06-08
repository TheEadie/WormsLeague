using System.Globalization;
using System.IO.Abstractions;
using ImageMagick;

namespace Worms.Armageddon.Game.Fake;

internal sealed class Installed : IRecordingWormsArmageddon
{
    private static readonly byte[] FrameContent = BuildPngBytes();

    private readonly string _path;
    private readonly Version _version;
    private readonly bool _hostCreatesReplay;
    private readonly List<PlayReplayCall> _playReplayCalls = [];

    internal IFileSystem FileSystem { get; }

    public IReadOnlyList<PlayReplayCall> PlayReplayCalls => _playReplayCalls;

    public int HostCallCount { get; private set; }

    public Installed(
        IFileSystem fileSystem,
        string? path = null,
        Version? version = null,
        bool hostCreatesReplay = true)
    {
        FileSystem = fileSystem;
        _hostCreatesReplay = hostCreatesReplay;
        _path = path ?? @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon";
        _version = version ?? new Version(1, 0, 0, 0);

        _ = FileSystem.Directory.CreateDirectory(_path);
        _ = FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(_path, "User"));
        _ = FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(_path, "User", "Schemes"));
        _ = FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(_path, "User", "Games"));
        _ = FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(_path, "User", "Capture"));
        FileSystem.File.WriteAllBytes(FileSystem.Path.Combine(_path, "WA.exe"), []);
    }

    public GameInfo FindInstallation() =>
        new(
            true,
            FileSystem.Path.Combine(_path, "WA.exe"),
            "WA",
            _version,
            FileSystem.Path.Combine(_path, "User", "Schemes"),
            FileSystem.Path.Combine(_path, "User", "Games"),
            FileSystem.Path.Combine(_path, "User", "Capture"));

    public Task Host()
    {
        HostCallCount++;
        if (_hostCreatesReplay)
        {
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
            FileSystem.File.WriteAllBytes(
                FileSystem.Path.Combine(_path, "User", "Games", $"{dateTime} [Offline] 1-UP, 2-UP.WAGame"),
                []);
        }

        return Task.CompletedTask;
    }

    public Task GenerateReplayLog(string replayPath)
    {
        if (FileSystem.File.Exists(replayPath))
        {
            FileSystem.File.WriteAllBytes(
                replayPath.Replace(".WAGame", ".log", StringComparison.InvariantCultureIgnoreCase),
                []);
        }

        return Task.CompletedTask;
    }

    public Task PlayReplay(string replayPath)
    {
        _playReplayCalls.Add(new PlayReplayCall(replayPath, null));
        return Task.CompletedTask;
    }

    public Task PlayReplay(string replayPath, TimeSpan startTime)
    {
        _playReplayCalls.Add(new PlayReplayCall(replayPath, startTime));
        return Task.CompletedTask;
    }

    public async Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480)
    {
        if (!FileSystem.File.Exists(replayPath))
        {
            return;
        }

        // WA strips the .WAGame extension when naming the capture folder, but keeps any other extension.
        var fileName = FileSystem.Path.GetFileName(replayPath);
        var captureFolderName = fileName.EndsWith(".WAGame", StringComparison.InvariantCultureIgnoreCase)
            ? FileSystem.Path.GetFileNameWithoutExtension(fileName)
            : fileName;

        var framesFolder = FileSystem.Path.Combine(_path, "User", "Capture", captureFolderName);
        _ = FileSystem.Directory.CreateDirectory(framesFolder);

        var frames = (int)((endTime - startTime).TotalSeconds * fps);
        for (var i = 0; i < frames; i++)
        {
            var framePath = FileSystem.Path.Combine(framesFolder, $"video_{i:D6}.png");
            await FileSystem.File.WriteAllBytesAsync(framePath, FrameContent);
        }
    }

    private static byte[] BuildPngBytes()
    {
        using var image = new MagickImage(MagickColors.Black, 16, 16);
        return image.ToByteArray(MagickFormat.Png);
    }
}

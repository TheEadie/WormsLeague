using System.Globalization;
using System.IO.Abstractions;
using ImageMagick;

namespace Worms.Armageddon.Game.Fake;

public sealed class Installed : IWormsArmageddon
{
    public const string MultiTurnLog = """
                                       Game Started at 2024-01-02 10:00:00 GMT
                                       Red: "a person" as "Some Team"
                                       Blue: "another person" as "Team 2"
                                       [00:06:59.08] ••• Some Team (a person) starts turn
                                       [00:07:08.26] ••• Some Team (a person) fires Shotgun
                                       [00:07:26.60] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat
                                       [00:09:59.08] ••• Team 2 (another person) starts turn
                                       [00:10:08.26] ••• Team 2 (another person) fires Shotgun
                                       [00:11:26.60] ••• Team 3 (another person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat
                                       """;

    private static readonly byte[] FrameContent = BuildPngBytes();

    private static readonly Lazy<byte[]> RedgateBytes = new(() =>
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "sample-data", "schemes", "redgate.wsc");
        return File.ReadAllBytes(path);
    });

    private readonly string _path;
    private readonly Version _version;
    private readonly IFileSystem _fileSystem;
    private readonly bool _hostCreatesReplay;
    private readonly List<PlayReplayCall> _playReplayCalls = [];

    public IReadOnlyList<PlayReplayCall> PlayReplayCalls => _playReplayCalls;

    public bool HostWasCalled { get; private set; }

    public Installed(
        IFileSystem fileSystem,
        string? path = null,
        Version? version = null,
        bool hostCreatesReplay = true)
    {
        _fileSystem = fileSystem;
        _hostCreatesReplay = hostCreatesReplay;
        _path = path ?? @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon";
        _version = version ?? new Version(1, 0, 0, 0);

        _ = _fileSystem.Directory.CreateDirectory(_path);
        _ = _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_path, "User"));
        _ = _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_path, "User", "Schemes"));
        _ = _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_path, "User", "Games"));
        _ = _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_path, "User", "Capture"));
        _fileSystem.File.WriteAllBytes(_fileSystem.Path.Combine(_path, "WA.exe"), []);
    }

    public GameInfo FindInstallation() =>
        new(
            true,
            _fileSystem.Path.Combine(_path, "WA.exe"),
            "WA",
            _version,
            _fileSystem.Path.Combine(_path, "User", "Schemes"),
            _fileSystem.Path.Combine(_path, "User", "Games"),
            _fileSystem.Path.Combine(_path, "User", "Capture"));

    public Task Host()
    {
        HostWasCalled = true;
        if (_hostCreatesReplay)
        {
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
            _fileSystem.File.WriteAllBytes(
                _fileSystem.Path.Combine(_path, "User", "Games", $"{dateTime} [Offline] 1-UP, 2-UP.WAGame"),
                []);
        }

        return Task.CompletedTask;
    }

    public Task GenerateReplayLog(string replayPath)
    {
        ArgumentNullException.ThrowIfNull(replayPath);
        if (_fileSystem.File.Exists(replayPath))
        {
            _fileSystem.File.WriteAllBytes(
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
        if (!_fileSystem.File.Exists(replayPath))
        {
            return;
        }

        // WA strips the .WAGame extension when naming the capture folder, but keeps any other extension.
        var fileName = _fileSystem.Path.GetFileName(replayPath);
        var captureFolderName = fileName.EndsWith(".WAGame", StringComparison.InvariantCultureIgnoreCase)
            ? _fileSystem.Path.GetFileNameWithoutExtension(fileName)
            : fileName;

        var framesFolder = _fileSystem.Path.Combine(_path, "User", "Capture", captureFolderName);
        _ = _fileSystem.Directory.CreateDirectory(framesFolder);

        var frames = (int)((endTime - startTime).TotalSeconds * fps);
        for (var i = 0; i < frames; i++)
        {
            var framePath = _fileSystem.Path.Combine(framesFolder, $"video_{i:D6}.png");
            await _fileSystem.File.WriteAllBytesAsync(framePath, FrameContent);
        }
    }

    /// <summary>
    /// Writes an empty replay file (and optional .log) into the fake's replay folder so the CLI can discover it.
    /// </summary>
    public void WriteReplay(string filenameNoExt, string? logContent = null)
    {
        var info = FindInstallation();
        _fileSystem.File.WriteAllBytes(
            _fileSystem.Path.Combine(info.ReplayFolder, filenameNoExt + ".WAgame"), []);
        if (logContent is not null)
        {
            _fileSystem.File.WriteAllText(
                _fileSystem.Path.Combine(info.ReplayFolder, filenameNoExt + ".log"), logContent);
        }
    }

    /// <summary>
    /// Writes the bytes of sample-data/schemes/redgate.wsc into the fake's schemes folder at
    /// &lt;SchemesFolder&gt;/&lt;schemeName&gt;.wsc so LocalSchemesRetriever can discover it.
    /// </summary>
    public void WriteScheme(string schemeName)
    {
        var info = FindInstallation();
        var path = _fileSystem.Path.Combine(info.SchemesFolder, schemeName + ".wsc");
        _fileSystem.File.WriteAllBytes(path, RedgateBytes.Value);
    }

    private static byte[] BuildPngBytes()
    {
        using var image = new MagickImage(MagickColors.Black, 16, 16);
        return image.ToByteArray(MagickFormat.Png);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docker-compose.yaml")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not find repo root (docker-compose.yaml not found)");
    }
}

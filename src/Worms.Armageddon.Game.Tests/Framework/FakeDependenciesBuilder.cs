using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Worms.Armageddon.Game.Win;
using IFileVersionInfo = Worms.Armageddon.Game.System.IFileVersionInfo;

namespace Worms.Armageddon.Game.Tests.Framework;

internal sealed class FakeDependenciesBuilder : IWormsArmageddonBuilder
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly IFileVersionInfo _fileVersionInfo = Substitute.For<IFileVersionInfo>();
    private readonly IRegistry _registry = Substitute.For<IRegistry>();
    private readonly IWormsRunner _wormsRunner = Substitute.For<IWormsRunner>();

    private bool _hostCreatesReplay = true;
    private bool _isInstalled = true;

    private string _path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\"
        : Environment.GetEnvironmentVariable("HOME") + "/.wine/drive_c/WA/";

    private Version _version = new(1, 0, 0, 0);

    public IWormsArmageddonBuilder WhereHostCmdDoesNotCreateReplayFile()
    {
        _hostCreatesReplay = false;
        return this;
    }

    public IWormsArmageddonBuilder Installed(string? path = null, Version? version = null)
    {
        _isInstalled = true;
        _path = path ?? _path;
        _version = version ?? _version;
        return this;
    }

    public IWormsArmageddonBuilder NotInstalled()
    {
        _isInstalled = false;
        return this;
    }

    public IWormsArmageddonBuilder WithReplayFilePath(string replayFilePath)
    {
        _fileSystem.AddEmptyFile(replayFilePath);
        return this;
    }

    public IFileSystem GetFileSystem() => _fileSystem;

    public IWormsArmageddon Build()
    {
        if (_isInstalled)
        {
            MockInstallation(_path, _version);
        }
        else
        {
            MockNotInstalled();
        }

        return new ServiceCollection().AddWormsArmageddonGameServices()
            .AddScoped<IRegistry>(_ => _registry)
            .AddScoped<IFileVersionInfo>(_ => _fileVersionInfo)
            .AddScoped<IWormsRunner>(_ => _wormsRunner)
            .AddScoped<IFileSystem>(_ => _fileSystem)
            .BuildServiceProvider()
            .GetRequiredService<IWormsArmageddon>();
    }

    private void MockNotInstalled()
    {
        _ = _registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns((string?) null);
        _ = _fileVersionInfo.GetVersionInfo(Arg.Any<string>()).Returns(new Version(0, 0));
        _ = _wormsRunner.RunWorms(Arg.Any<string[]>())
            .ThrowsAsync(new InvalidOperationException("Worms Armageddon is not installed"));
    }

    private void MockInstallation(string path, Version version)
    {
        _ = _registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns(path);

        _fileSystem.AddDirectory(path);
        _fileSystem.AddDirectory(Path.Combine(path, "User"));
        _fileSystem.AddDirectory(Path.Combine(path, "User", "Schemes"));
        _fileSystem.AddDirectory(Path.Combine(path, "User", "Games"));
        _fileSystem.AddDirectory(Path.Combine(path, "User", "Capture"));
        _fileSystem.AddFile(Path.Combine(path, "WA.exe"), new MockFileData([]));

        _ = _fileVersionInfo.GetVersionInfo(Path.Combine(path, "WA.exe")).Returns(version);

        _ = _wormsRunner.RunWorms("wa://").Returns(Task.CompletedTask).AndDoes(_ => MockHost(path));

        _ = _wormsRunner.RunWorms("/getlog", Arg.Any<string>(), "/quiet")
            .Returns(Task.CompletedTask)
            .AndDoes(x => MockGenerateLogFile(x.ArgAt<string[]>(0)[1]));

        _ = _wormsRunner.RunWorms(
                "/getvideo",
                Arg.Any<string>(), // replay file path
                Arg.Any<string>(), // fps
                Arg.Any<string>(), // start time
                Arg.Any<string>(), // end time
                Arg.Any<string>(), // x resolution
                Arg.Any<string>(), // y resolution
                "/quiet")
            .Returns(Task.CompletedTask)
            .AndDoes(
                x => MockExtractReplayFrames(
                    x.ArgAt<string[]>(0)[1],
                    int.Parse(x.ArgAt<string[]>(0)[2], CultureInfo.InvariantCulture),
                    TimeSpan.Parse(x.ArgAt<string[]>(0)[3], CultureInfo.InvariantCulture),
                    TimeSpan.Parse(x.ArgAt<string[]>(0)[4], CultureInfo.InvariantCulture)));
    }

    private void MockHost(string path)
    {
        if (!_hostCreatesReplay)
        {
            return;
        }

        var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
        _fileSystem.AddEmptyFile(Path.Combine(path, "User", "Games", $"{dateTime} [Offline] 1-UP, 2-UP.WAGame"));
    }

    private void MockGenerateLogFile(string replayFilePath)
    {
        var replayFilePathCleaned = replayFilePath.Replace("\"", string.Empty, StringComparison.InvariantCulture);

        if (!_fileSystem.File.Exists(replayFilePathCleaned))
        {
            return;
        }

        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(replayFilePathCleaned);
        var folder = _fileSystem.Path.GetDirectoryName(replayFilePathCleaned);
        var logFilePath = Path.Combine(folder!, $"{fileName}.log");
        _fileSystem.AddEmptyFile(logFilePath);
    }

    private void MockExtractReplayFrames(string replayFilePath, int fps, TimeSpan startTime, TimeSpan endTime)
    {
        var replayFilePathCleaned = replayFilePath.Replace("\"", string.Empty, StringComparison.InvariantCulture);

        if (!_fileSystem.File.Exists(replayFilePathCleaned))
        {
            return;
        }

        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(replayFilePathCleaned);
        var frames = fps * (endTime - startTime).TotalSeconds;
        for (var i = 0; i < frames; i++)
        {
            var frameFilePath = Path.Combine(_path, "User", "Capture", $"{fileName}_{i,4}.png");
            _fileSystem.AddEmptyFile(frameFilePath);
        }
    }
}

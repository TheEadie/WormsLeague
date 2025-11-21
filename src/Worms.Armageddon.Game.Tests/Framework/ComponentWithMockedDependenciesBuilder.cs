using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.Core;
using Worms.Armageddon.Game.System;
using Worms.Armageddon.Game.Win;
using IFileVersionInfo = Worms.Armageddon.Game.System.IFileVersionInfo;

namespace Worms.Armageddon.Game.Tests.Framework;

internal sealed class ComponentWithMockedDependenciesBuilder : IWormsArmageddonBuilder
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly IFileVersionInfo _fileVersionInfo = Substitute.For<IFileVersionInfo>();
    private readonly IRegistry _registry = Substitute.For<IRegistry>();
    private readonly IProcessRunner _processRunner = Substitute.For<IProcessRunner>();

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
            .AddScoped<ISteamService>(_ => Substitute.For<ISteamService>()) // This has a thread.Sleep in it
            .AddScoped<IRegistry>(_ => _registry)
            .AddScoped<IFileVersionInfo>(_ => _fileVersionInfo)
            .AddScoped<IProcessRunner>(_ => _processRunner)
            .AddScoped<IFileSystem>(_ => _fileSystem)
            .AddLogging()
            .BuildServiceProvider()
            .GetRequiredService<IWormsArmageddon>();
    }

    private void MockNotInstalled()
    {
        _ = _registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns((string?) null);
        _ = _fileVersionInfo.GetVersionInfo(Arg.Any<string>()).Returns(new Version(0, 0));
    }

    private void MockInstallation(string path, Version version)
    {
        _ = _registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns(path);

        var wormsExe = Path.Combine(path, "WA.exe");
        _fileSystem.AddDirectory(path);
        _fileSystem.AddDirectory(Path.Combine(path, "User"));
        _fileSystem.AddDirectory(Path.Combine(path, "User", "Schemes"));
        _fileSystem.AddDirectory(Path.Combine(path, "User", "Games"));
        _fileSystem.AddDirectory(Path.Combine(path, "User", "Capture"));
        _fileSystem.AddFile(wormsExe, new MockFileData([]));

        _ = _fileVersionInfo.GetVersionInfo(wormsExe).Returns(version);

        _ = _processRunner.Start(wormsExe, "wa://").Returns(Substitute.For<IProcess>()).AndDoes(_ => MockHost(path));
        _ = _processRunner.Start(Arg.Is<ProcessStartInfo>(x => x.Arguments.Contains("wa://")))
            .Returns(Substitute.For<IProcess>())
            .AndDoes(_ => MockHost(path));

        _ = _processRunner.Start(wormsExe, "/getlog", Arg.Any<string>(), "/quiet")
            .Returns(Substitute.For<IProcess>())
            .AndDoes(x => MockGenerateLogFile(GetProcessRunnerArgAt(x, 1)));
        _ = _processRunner.Start(Arg.Is<ProcessStartInfo>(x => x.Arguments.Contains("/getlog")))
            .Returns(Substitute.For<IProcess>())
            .AndDoes(x => MockGenerateLogFile(GetProcessRunnerArgAt(x, 1)));

        _ = _processRunner.Start(
                wormsExe,
                "/getvideo",
                Arg.Any<string>(), // replay file path
                Arg.Any<string>(), // fps
                Arg.Any<string>(), // start time
                Arg.Any<string>(), // end time
                Arg.Any<string>(), // x resolution
                Arg.Any<string>(), // y resolution
                "/quiet")
            .Returns(Substitute.For<IProcess>())
            .AndDoes(
                x => MockExtractReplayFrames(
                    GetProcessRunnerArgAt(x, 1),
                    int.Parse(GetProcessRunnerArgAt(x, 2), CultureInfo.InvariantCulture),
                    TimeSpan.Parse(GetProcessRunnerArgAt(x, 3), CultureInfo.InvariantCulture),
                    TimeSpan.Parse(GetProcessRunnerArgAt(x, 4), CultureInfo.InvariantCulture)));
        _ = _processRunner.Start(Arg.Is<ProcessStartInfo>(x => x.Arguments.Contains("/getvideo")))
            .Returns(Substitute.For<IProcess>())
            .AndDoes(
                x => MockExtractReplayFrames(
                    GetProcessRunnerArgAt(x, 1),
                    int.Parse(GetProcessRunnerArgAt(x, 2), CultureInfo.InvariantCulture),
                    TimeSpan.Parse(GetProcessRunnerArgAt(x, 3), CultureInfo.InvariantCulture),
                    TimeSpan.Parse(GetProcessRunnerArgAt(x, 4), CultureInfo.InvariantCulture)));
    }

    private static string GetProcessRunnerArgAt(CallInfo callInfo, int pos) =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? callInfo.ArgAt<string[]>(1)[pos]
            : callInfo.ArgAt<ProcessStartInfo>(0).Arguments.Split(" ")[pos + 4];

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
        var replayFilePathCleaned = replayFilePath.Trim('"').Trim('\'');

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
        var replayFilePathCleaned = replayFilePath.Trim('"').Trim('\'');

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

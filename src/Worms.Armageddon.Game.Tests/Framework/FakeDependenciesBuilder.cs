using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
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
    private string _path = @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\";
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
}

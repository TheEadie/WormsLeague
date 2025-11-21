using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game.Fake;

namespace Worms.Armageddon.Game.Tests.Framework;

internal sealed class FakeComponentBuilder : IWormsArmageddonBuilder
{
    private readonly MockFileSystem _fileSystem = new();

    private bool _hostCreatesReplay = true;
    private bool _isInstalled = true;
    private string? _path;
    private Version? _version;

    public IWormsArmageddonBuilder WhereHostCmdDoesNotCreateReplayFile()
    {
        _hostCreatesReplay = false;
        return this;
    }

    public IWormsArmageddonBuilder Installed(string? path = null, Version? version = null)
    {
        _isInstalled = true;
        _path = path;
        _version = version;
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
        var services = new ServiceCollection();
        _ = _isInstalled
            ? services.AddFakeInstalledWormsArmageddonServices(_fileSystem, _path, _version, _hostCreatesReplay)
            : services.AddFakeNotInstalledWormsArmageddonServices();

        return services.BuildServiceProvider().GetRequiredService<IWormsArmageddon>();
    }
}

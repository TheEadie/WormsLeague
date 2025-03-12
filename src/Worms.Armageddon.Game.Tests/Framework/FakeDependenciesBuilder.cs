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
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly MockFileSystem _fileSystem = new();

    public FakeDependenciesBuilder()
    {
        _ = _services.AddWormsArmageddonGameServices();
        _ = Installed(); // Default to installed
    }

    public IWormsArmageddonBuilder Installed(string? path = null, Version? version = null)
    {
        path ??= @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\";
        version ??= new Version(1, 0, 0, 0);
        var exePath = Path.Combine(path, "WA.exe");

        var registry = Substitute.For<IRegistry>();
        _ = registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns(path);

        _fileSystem.AddEmptyFile(exePath);

        var fileVersionInfo = Substitute.For<IFileVersionInfo>();
        _ = fileVersionInfo.GetVersionInfo(exePath).Returns(version);

        var wormsRunner = Substitute.For<IWormsRunner>();
        _ = wormsRunner.RunWorms("wa://")
            .Returns(Task.CompletedTask)
            .AndDoes(_ => _fileSystem.AddEmptyFile(Path.Combine(path, "User", "Games", "replay.WAGame")));

        _ = _services.AddScoped<IRegistry>(_ => registry)
            .AddScoped<IFileVersionInfo>(_ => fileVersionInfo)
            .AddScoped<IWormsRunner>(_ => wormsRunner);
        return this;
    }

    public IWormsArmageddonBuilder NotInstalled()
    {
        var registry = Substitute.For<IRegistry>();
        _ = registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns((string?) null);

        var fileVersionInfo = Substitute.For<IFileVersionInfo>();
        _ = fileVersionInfo.GetVersionInfo(Arg.Any<string>()).Returns(new Version(0, 0));

        var wormsRunner = Substitute.For<IWormsRunner>();
        _ = wormsRunner.RunWorms(Arg.Any<string[]>())
            .ThrowsAsync(new InvalidOperationException("Worms Armageddon is not installed"));

        _ = _services.AddScoped<IRegistry>(_ => registry)
            .AddScoped<IFileVersionInfo>(_ => fileVersionInfo)
            .AddScoped<IWormsRunner>(_ => wormsRunner);
        return this;
    }

    public IFileSystem GetFileSystem() => _fileSystem;

    public IWormsArmageddon Build()
    {
        _ = _services.AddScoped<IFileSystem>(_ => _fileSystem);
        var serviceProvider = _services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IWormsArmageddon>();
    }
}

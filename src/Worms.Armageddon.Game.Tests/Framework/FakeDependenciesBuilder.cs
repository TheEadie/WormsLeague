using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Worms.Armageddon.Game.System;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game.Tests.Framework;

internal sealed class FakeDependenciesBuilder : IWormsArmageddonBuilder
{
    private readonly IServiceCollection _services = new ServiceCollection();

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

        var fileSystem = Substitute.For<IFileSystem>();
        _ = fileSystem.File.Exists(exePath).Returns(true);

        var fileVersionInfo = Substitute.For<IFileVersionInfo>();
        _ = fileVersionInfo.GetVersionInfo(exePath).Returns(version);

        var wormsRunner = Substitute.For<IWormsRunner>();
        _ = wormsRunner.RunWorms("wa://").Returns(Task.CompletedTask);

        _ = _services.AddScoped<IRegistry>(_ => registry)
            .AddScoped<IFileVersionInfo>(_ => fileVersionInfo)
            .AddScoped<IWormsRunner>(_ => wormsRunner)
            .AddScoped<IFileSystem>(_ => fileSystem);
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

    public IWormsArmageddon Build()
    {
        var serviceProvider = _services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IWormsArmageddon>();
    }
}

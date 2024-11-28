using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Worms.Armageddon.Game.System;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game.Tests.Framework;

internal static class FakeDependencies
{
    public static IWormsArmageddon GetWormsArmageddonApi(InstallationType installationType)
    {
        var services = new ServiceCollection().AddWormsArmageddonGameServices();
        services = installationType switch
        {
            InstallationType.NotInstalled => services.AddNotInstalledWormsArmageddon(),
            InstallationType.Installed => services.AddInstalledWormsArmageddon(),
            _ => throw new ArgumentOutOfRangeException(nameof(installationType), installationType, null)
        };
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IWormsArmageddon>();
    }

    private static IServiceCollection AddNotInstalledWormsArmageddon(this IServiceCollection builder)
    {
        var registry = Substitute.For<IRegistry>();
        _ = registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns((string?) null);

        var fileVersionInfo = Substitute.For<IFileVersionInfo>();
        _ = fileVersionInfo.GetVersionInfo(Arg.Any<string>()).Returns(new Version(0, 0));

        var wormsRunner = Substitute.For<IWormsRunner>();
        _ = wormsRunner.RunWorms(Arg.Any<string[]>())
            .ThrowsAsync(new InvalidOperationException("Worms Armageddon is not installed"));

        return builder.AddScoped<IRegistry>(_ => registry)
            .AddScoped<IFileVersionInfo>(_ => fileVersionInfo)
            .AddScoped<IWormsRunner>(_ => wormsRunner);
    }

    private static IServiceCollection AddInstalledWormsArmageddon(this IServiceCollection builder)
    {
        var linuxUserHome = Environment.GetEnvironmentVariable("HOME");

        var registry = Substitute.For<IRegistry>();
        _ = registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\");

        var fileSystem = Substitute.For<IFileSystem>();
        _ = fileSystem.File.Exists(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\WA.exe")
            .Returns(true);
        _ = fileSystem.File.Exists($"{linuxUserHome}/.wine/drive_c/WA/WA.exe").Returns(true);

        var currentVersion = new Version(3, 8, 1, 0);
        var fileVersionInfo = Substitute.For<IFileVersionInfo>();
        _ = fileVersionInfo.GetVersionInfo(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\WA.exe")
            .Returns(currentVersion);
        _ = fileVersionInfo.GetVersionInfo($"{linuxUserHome}/.wine/drive_c/WA/WA.exe").Returns(currentVersion);

        var wormsRunner = Substitute.For<IWormsRunner>();
        _ = wormsRunner.RunWorms("wa://").Returns(Task.CompletedTask);

        return builder.AddScoped<IRegistry>(_ => registry)
            .AddScoped<IFileVersionInfo>(_ => fileVersionInfo)
            .AddScoped<IWormsRunner>(_ => wormsRunner)
            .AddScoped<IFileSystem>(_ => fileSystem);
    }
}

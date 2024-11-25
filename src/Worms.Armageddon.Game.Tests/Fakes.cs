using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game.Tests;

internal static class Fakes
{
    internal enum InstallationType
    {
        NotInstalled = 0,
        Installed = 1
    }

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

        return builder.AddScoped<IRegistry>(_ => registry).AddScoped<IFileVersionInfo>(_ => fileVersionInfo);
    }

    private static IServiceCollection AddInstalledWormsArmageddon(this IServiceCollection builder)
    {
        var registry = Substitute.For<IRegistry>();
        _ = registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null)
            .Returns(@"C:\WormsArmageddon");

        var fileVersionInfo = Substitute.For<IFileVersionInfo>();
        _ = fileVersionInfo.GetVersionInfo(@"C:\WormsArmageddon\WA.exe").Returns(new Version(3, 8, 0, 0));

        return builder.AddScoped<IRegistry>(_ => registry).AddScoped<IFileVersionInfo>(_ => fileVersionInfo);
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game.Replays;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game;

public static class ServiceRegistration
{
    public static IServiceCollection AddWormsArmageddonGameServices(this IServiceCollection builder) =>
        builder.AddOsServices()
            .AddScoped<IReplayFrameExtractor, ReplayFrameExtractor>()
            .AddScoped<IReplayLogGenerator, ReplayLogGenerator>()
            .AddScoped<IReplayPlayer, ReplayPlayer>();

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
    private static IServiceCollection AddOsServices(this IServiceCollection builder)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return builder.AddWindowsServices();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return builder.AddLinuxServices();
        }

        throw new PlatformNotSupportedException("This platform is not supported");
    }

    [SupportedOSPlatform("windows")]
    private static IServiceCollection AddWindowsServices(this IServiceCollection builder) =>
        builder.AddScoped<ISteamService, SteamService>()
            .AddScoped<IWormsLocator, WormsLocator>()
            .AddScoped<IWormsRunner, WormsRunner>();

    private static IServiceCollection AddLinuxServices(this IServiceCollection builder) =>
        builder.AddScoped<IWormsLocator, Linux.WormsLocator>().AddScoped<IWormsRunner, Linux.WormsRunner>();
}

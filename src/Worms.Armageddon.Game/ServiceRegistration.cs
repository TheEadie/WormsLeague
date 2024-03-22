using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game.Replays;

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
}

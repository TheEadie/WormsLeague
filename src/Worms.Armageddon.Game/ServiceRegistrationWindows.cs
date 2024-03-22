using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game;

[SupportedOSPlatform("windows")]
internal static class ServiceRegistrationWindows
{
    public static IServiceCollection AddWindowsServices(this IServiceCollection builder) =>
        builder.AddTransient<ISteamService, SteamService>()
            .AddTransient<IWormsLocator, WormsLocator>()
            .AddTransient<IWormsRunner, WormsRunner>();
}

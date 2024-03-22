using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game;

[SupportedOSPlatform("windows")]
internal static class ServiceRegistrationWindows
{
    public static IServiceCollection AddWindowsServices(this IServiceCollection builder) =>
        builder.AddScoped<ISteamService, SteamService>()
            .AddScoped<IWormsLocator, WormsLocator>()
            .AddScoped<IWormsRunner, WormsRunner>();
}

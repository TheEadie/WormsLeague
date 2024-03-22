using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game.Linux;

namespace Worms.Armageddon.Game;

internal static class ServiceRegistrationLinux
{
    public static IServiceCollection AddLinuxServices(this IServiceCollection builder) =>
        builder.AddScoped<IWormsLocator, WormsLocator>().AddScoped<IWormsRunner, WormsRunner>();
}

using Microsoft.Extensions.DependencyInjection;

namespace Worms.Armageddon.Game.Fake;

public static class ServiceRegistration
{
    public static IServiceCollection AddFakeInstalledWormsArmageddonServices(
        this IServiceCollection builder,
        string? gamePath = null,
        Version? version = null)
    {
        var installed = new Installed(gamePath, version);
        return builder.AddScoped<IWormsArmageddon>(_ => installed);
    }

    public static IServiceCollection AddFakeNotInstalledWormsArmageddonServices(
        this IServiceCollection builder) => builder.AddScoped<IWormsArmageddon, NotInstalled>();
}

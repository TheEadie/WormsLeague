using Microsoft.Extensions.DependencyInjection;

namespace Worms.Armageddon.Game.Fake;

public static class ServiceRegistration
{
    public static IServiceCollection AddFakeWormsArmageddonServices(
        this IServiceCollection builder,
        FakeConfiguration fakeConfiguration)
    {
        ArgumentNullException.ThrowIfNull(fakeConfiguration);
        return fakeConfiguration.IsInstalled
            ? builder.AddScoped<IWormsArmageddon, Installed>()
            : builder.AddScoped<IWormsArmageddon, NotInstalled>();
    }
}

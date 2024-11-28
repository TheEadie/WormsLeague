using Microsoft.Extensions.DependencyInjection;

namespace Worms.Armageddon.Game.Tests.Framework;

internal static class RealDependencies
{
    public static IWormsArmageddon GetWormsArmageddonApi()
    {
        var services = new ServiceCollection().AddWormsArmageddonGameServices();
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IWormsArmageddon>();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game.Fake;

namespace Worms.Armageddon.Game.Tests.Framework;

internal static class FakeComponent
{
    public static IWormsArmageddon GetWormsArmageddonApi(FakeConfiguration configuration)
    {
        var services = new ServiceCollection().AddFakeWormsArmageddonServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IWormsArmageddon>();
    }
}

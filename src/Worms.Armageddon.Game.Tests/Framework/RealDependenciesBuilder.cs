using Microsoft.Extensions.DependencyInjection;

namespace Worms.Armageddon.Game.Tests.Framework;

internal sealed class RealDependenciesBuilder : IWormsArmageddonBuilder
{
    public IWormsArmageddonBuilder Installed(string? path = null, Version? version = null)
    {
        Console.Error.WriteLine("Warning: Running unit test with real dependencies. Ensure your test environment is set up correctly.");
        return this;
    }

    public IWormsArmageddonBuilder NotInstalled()
    {
        Console.Error.WriteLine("Warning: Running unit test with real dependencies. Ensure your test environment is set up correctly.");
        return this;
    }

    public IWormsArmageddon Build()
    {
        var services = new ServiceCollection().AddWormsArmageddonGameServices();
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IWormsArmageddon>();
    }
}

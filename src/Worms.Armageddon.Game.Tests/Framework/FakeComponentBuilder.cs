using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Game.Fake;

namespace Worms.Armageddon.Game.Tests.Framework;

internal sealed class FakeComponentBuilder : IWormsArmageddonBuilder
{
    private readonly IServiceCollection _services = new ServiceCollection();

    public FakeComponentBuilder() => _ = Installed(); // Default to installed

    public IWormsArmageddonBuilder Installed(string? path = null, Version? version = null)
    {
        _ = _services.AddFakeInstalledWormsArmageddonServices(path, version);
        return this;
    }

    public IWormsArmageddonBuilder NotInstalled()
    {
        _ = _services.AddFakeNotInstalledWormsArmageddonServices();
        return this;
    }

    public IWormsArmageddon Build()
    {
        var serviceProvider = _services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IWormsArmageddon>();
    }
}

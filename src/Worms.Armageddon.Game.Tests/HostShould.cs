using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace Worms.Armageddon.Game.Tests;

public class HostShould
{
    private readonly IWormsArmageddon _wormsArmageddon;
    private readonly IWormsRunner _runner;

    public HostShould()
    {
        _runner = Substitute.For<IWormsRunner>();
        var services = new ServiceCollection();
        _ = services.AddWormsArmageddonGameServices();
        _ = services.AddScoped<IWormsRunner>(_ => _runner);
        var serviceProvider = services.BuildServiceProvider();
        _wormsArmageddon = serviceProvider.GetRequiredService<IWormsArmageddon>();
    }

    [Test]
    public async Task LaunchWormsArmageddon()
    {
        await _wormsArmageddon.Host().ConfigureAwait(false);
        await _runner.Received().RunWorms("wa://").ConfigureAwait(false);
    }
}

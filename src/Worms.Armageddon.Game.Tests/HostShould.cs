using NUnit.Framework;
using Shouldly;

namespace Worms.Armageddon.Game.Tests;

internal sealed class HostShould
{
    [Test]
    public async Task LaunchWormsArmageddon()
    {
        var wormsArmageddon = Fakes.GetWormsArmageddonApi(Fakes.InstallationType.Installed);
        await wormsArmageddon.Host().ConfigureAwait(false);
    }

    [Test]
    public async Task ErrorWhenNotInstalled()
    {
        var wormsArmageddon = Fakes.GetWormsArmageddonApi(Fakes.InstallationType.NotInstalled);
        _ = await Should.ThrowAsync<InvalidOperationException>(wormsArmageddon.Host()).ConfigureAwait(false);
    }
}

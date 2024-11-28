using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Tests.Framework;

namespace Worms.Armageddon.Game.Tests;

[FakeDependencies]
[RealDependencies]
internal sealed class HostShould(ApiType apiType)
{
    [Test]
    public async Task LaunchWormsArmageddon()
    {
        var wormsArmageddon = Api.GetWormsArmageddon(apiType, InstallationType.Installed);
        await wormsArmageddon.Host().ConfigureAwait(false);
    }

    [Test]
    public async Task ErrorWhenNotInstalled()
    {
        var wormsArmageddon = Api.GetWormsArmageddon(apiType, InstallationType.NotInstalled);
        var exception = await Should.ThrowAsync<InvalidOperationException>(wormsArmageddon.Host())
            .ConfigureAwait(false);
        exception.Message.ShouldBe("Worms Armageddon is not installed");
    }
}

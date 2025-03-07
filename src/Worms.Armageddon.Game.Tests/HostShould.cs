using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Fake;
using Worms.Armageddon.Game.Tests.Framework;

namespace Worms.Armageddon.Game.Tests;

[FakeDependencies]
[RealDependencies]
[FakeComponent]
internal sealed class HostShould(ApiType apiType)
{
    [Test]
    public async Task LaunchWormsArmageddon()
    {
        var wormsArmageddon = Api.GetWormsArmageddon(apiType, new FakeConfiguration(true));
        await wormsArmageddon.Host();
    }

    [Test]
    public async Task ErrorWhenNotInstalled()
    {
        var wormsArmageddon = Api.GetWormsArmageddon(apiType, new FakeConfiguration(false));
        var exception = await Should.ThrowAsync<InvalidOperationException>(wormsArmageddon.Host());
        exception.Message.ShouldBe("Worms Armageddon is not installed");
    }
}

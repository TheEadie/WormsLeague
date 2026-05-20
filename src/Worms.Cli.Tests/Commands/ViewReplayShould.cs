using NUnit.Framework;
using Shouldly;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class ViewReplayShould
{
    [Test]
    public async Task LaunchTheReplayInWormsArmageddon()
    {
        using var host = new TestHost();
        ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two", ReplayFixtures.MultiTurnLog);

        var exitCode = await host.Run("view", "replay", "2024-01-02");

        exitCode.ShouldBe(0);
        host.WormsArmageddon.PlayReplayCalls.ShouldHaveSingleItem();
        var call = host.WormsArmageddon.PlayReplayCalls[0];
        call.Path.ShouldEndWith("2024-01-02 10.00.00 [Offline] One, Two.WAgame");
        call.StartTime.ShouldBeNull();
    }

    [Test]
    public async Task LaunchTheReplayAtTheRequestedTurn()
    {
        using var host = new TestHost();
        ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two", ReplayFixtures.MultiTurnLog);

        var exitCode = await host.Run("view", "replay", "2024-01-02", "--turn", "2");

        exitCode.ShouldBe(0);
        host.WormsArmageddon.PlayReplayCalls.ShouldHaveSingleItem();
        var call = host.WormsArmageddon.PlayReplayCalls[0];
        call.StartTime.ShouldBe(new TimeSpan(0, 0, 9, 59, 80));
    }

    [Test]
    public async Task ReturnNonZeroWhenNoReplayMatchesASpecificName()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("view", "replay", "nonexistent");

        exitCode.ShouldBe(1);
        host.WormsArmageddon.PlayReplayCalls.ShouldBeEmpty();
    }
}

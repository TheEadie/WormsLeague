using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Fake;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class GetReplayShould
{
    [TestCase("replay")]
    [TestCase("replays")]
    [TestCase("WAgame")]
    public async Task PrintReplayWithMatchingLog(string alias)
    {
        using var host = new TestHost();
        host.WormsArmageddon.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two", WormsArmageddonFakeExtensions.MultiTurnReplayLog);

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("get", alias, "2024-01-02");

        exitCode.ShouldBe(0);
        console.Output.ToString().ShouldContain("2024-01-02 10.00.00");
    }

    [Test]
    public async Task PrintReplayEvenWhenNoLogPresent()
    {
        using var host = new TestHost();
        host.WormsArmageddon.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two");

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("get", "replay", "2024-01-02");

        exitCode.ShouldBe(0);
        console.Output.ToString().ShouldContain("Replay has not been processed");
    }

    [Test]
    public async Task OrderResultsByDateDescending()
    {
        using var host = new TestHost();
        host.WormsArmageddon.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two");
        host.WormsArmageddon.WriteReplay("2024-02-15 12.00.00 [Offline] One, Two");

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("get", "replay", "*");

        exitCode.ShouldBe(0);
        var output = console.Output.ToString();
        output.IndexOf("2024-02-15", StringComparison.Ordinal)
            .ShouldBeLessThan(output.IndexOf("2024-01-02", StringComparison.Ordinal));
    }

    [Test]
    public async Task ReturnNonZeroWhenNoReplayMatchesASpecificName()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("get", "replay", "nonexistent");

        exitCode.ShouldBe(1);
    }
}

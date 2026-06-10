using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Fake;
using Worms.Cli.Resources.Local.Gifs;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class CreateGifShould
{
    [Test]
    public async Task ReturnNonZeroWhenNoReplayProvided()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("create", "gif", "--turn", "1");

        exitCode.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("No replay provided for the Gif being created"));
    }

    [Test]
    public async Task ReturnNonZeroWhenNoTurnProvided()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("create", "gif", "--replay", "2024-01-02");

        exitCode.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("No turn provided for the Gif being created"));
    }

    [Test]
    public async Task ReturnNonZeroWhenNoReplayMatchesPattern()
    {
        using var host = new TestHost();

        var exitCode = await host.Run("create", "gif", "--replay", "nonexistent", "--turn", "1");

        exitCode.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("No replays found with name: nonexistent"));
    }

    [Test]
    public async Task ReturnNonZeroWhenMoreThanOneReplayMatchesPattern()
    {
        using var host = new TestHost();
        host.WormsArmageddonSetup.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two");
        host.WormsArmageddonSetup.WriteReplay("2024-02-15 12.00.00 [Offline] One, Two");

        var exitCode = await host.Run("create", "gif", "--replay", "*", "--turn", "1");

        exitCode.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("More than one replay found matching pattern: *"));
    }

    [Test]
    public async Task ReturnNonZeroWhenReplayHasNoTurns()
    {
        using var host = new TestHost();
        host.WormsArmageddonSetup.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two");

        var exitCode = await host.Run("create", "gif", "--replay", "2024-01-02", "--turn", "1");

        exitCode.ShouldBe(1);
        host.Logs.Messages.ShouldContain(m => m.Message.Contains("Replay 2024-01-02 has no turns, cannot create gif"));
    }

    [Test]
    public async Task CreateGifWithBoundOptionsWhenReplayHasTurns()
    {
        using var host = new TestHost();
        host.WormsArmageddonSetup.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two", WormsArmageddonFakeSetup.MultiTurnReplayLog);
        host.GifCreator.Create(Arg.Any<LocalGifCreateParameters>(), Arg.Any<CancellationToken>())
            .Returns(new LocalGif("/captures/out.gif"));

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("create", "gif", "-r", "2024-01-02", "-t", "3", "-fps", "10", "-s", "4", "-so", "1", "-eo", "2");

        exitCode.ShouldBe(0);
        console.Output.ToString().ShouldContain("/captures/out.gif");
        await host.GifCreator.Received(1).Create(
            Arg.Is<LocalGifCreateParameters>(p =>
                p.Turn == 3u &&
                p.FramesPerSecond == 10u &&
                p.SpeedMultiplier == 4u &&
                p.StartOffset == TimeSpan.FromSeconds(1) &&
                p.EndOffset == TimeSpan.FromSeconds(2)),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateGifWithDefaultOptionsWhenOnlyReplayAndTurnGiven()
    {
        using var host = new TestHost();
        host.WormsArmageddonSetup.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two", WormsArmageddonFakeSetup.MultiTurnReplayLog);
        host.GifCreator.Create(Arg.Any<LocalGifCreateParameters>(), Arg.Any<CancellationToken>())
            .Returns(new LocalGif("/captures/out.gif"));

        using var _ = new ConsoleOutputScope();
        var exitCode = await host.Run("create", "gif", "-r", "2024-01-02", "-t", "1");

        exitCode.ShouldBe(0);
        await host.GifCreator.Received(1).Create(
            Arg.Is<LocalGifCreateParameters>(p =>
                p.Turn == 1u &&
                p.FramesPerSecond == 5u &&
                p.SpeedMultiplier == 2u &&
                p.StartOffset == TimeSpan.Zero &&
                p.EndOffset == TimeSpan.Zero),
            Arg.Any<CancellationToken>());
    }
}

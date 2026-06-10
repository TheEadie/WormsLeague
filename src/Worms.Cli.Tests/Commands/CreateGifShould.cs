using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Fake;
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
    public async Task CreateGifForTheSelectedTurnWithBoundOptions()
    {
        using var host = new TestHost();
        host.WormsArmageddonSetup.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two", WormsArmageddonFakeSetup.MultiTurnReplayLog);
        var captureFolder = host.WormsArmageddon.FindInstallation().CaptureFolder;
        StubGifCreator(host);

        using var console = new ConsoleOutputScope();
        var exitCode = await host.Run("create", "gif", "-r", "2024-01-02", "-t", "2", "-fps", "10", "-s", "4", "-so", "1", "-eo", "2");

        exitCode.ShouldBe(0);
        console.Output.ToString().Trim().ShouldBe(Path.Combine(captureFolder, "out.gif"));
        await host.GifCreator.Received(1).CreateGif(
            Arg.Is<string>(p => p.Contains("2024-01-02")),
            new TimeSpan(0, 0, 9, 59, 80),    // turn 2 start: 00:09:59.08
            new TimeSpan(0, 0, 11, 26, 600),  // turn 2 end:   00:11:26.60
            2,
            captureFolder,
            10u,
            4u,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2));
    }

    [Test]
    public async Task CreateGifForTheSelectedTurnWithDefaultOptions()
    {
        using var host = new TestHost();
        host.WormsArmageddonSetup.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two", WormsArmageddonFakeSetup.MultiTurnReplayLog);
        var captureFolder = host.WormsArmageddon.FindInstallation().CaptureFolder;
        StubGifCreator(host);

        using var _ = new ConsoleOutputScope();
        var exitCode = await host.Run("create", "gif", "-r", "2024-01-02", "-t", "1");

        exitCode.ShouldBe(0);
        await host.GifCreator.Received(1).CreateGif(
            Arg.Is<string>(p => p.Contains("2024-01-02")),
            new TimeSpan(0, 0, 6, 59, 80),    // turn 1 start: 00:06:59.08
            new TimeSpan(0, 0, 7, 26, 600),   // turn 1 end:   00:07:26.60
            1,
            captureFolder,
            5u,
            2u,
            TimeSpan.Zero,
            TimeSpan.Zero);
    }

    private static void StubGifCreator(TestHost host) =>
        host.GifCreator.CreateGif(
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<uint>(),
                Arg.Any<uint>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<TimeSpan?>())
            .Returns("out.gif");
}

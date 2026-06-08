using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Fake;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class DeleteReplayShould
{
    [Test]
    public async Task DeleteBothTheWAgameAndItsLog()
    {
        using var host = new TestHost();
        var replayFolder = host.WormsArmageddon.FindInstallation().ReplayFolder;
        host.WormsArmageddon.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two", WormsArmageddonFakeExtensions.MultiTurnReplayLog);

        var exitCode = await host.Run("delete", "replay", "2024-01-02");

        exitCode.ShouldBe(0);
        host.FileSystem.File.Exists(Path.Combine(replayFolder, "2024-01-02 10.00.00 [Offline] One, Two.WAgame")).ShouldBeFalse();
        host.FileSystem.File.Exists(Path.Combine(replayFolder, "2024-01-02 10.00.00 [Offline] One, Two.log")).ShouldBeFalse();
    }

    [Test]
    public async Task DeleteTheWAgameWhenNoLogPresent()
    {
        using var host = new TestHost();
        var replayFolder = host.WormsArmageddon.FindInstallation().ReplayFolder;
        host.WormsArmageddon.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two");

        var exitCode = await host.Run("delete", "replay", "2024-01-02");

        exitCode.ShouldBe(0);
        host.FileSystem.File.Exists(Path.Combine(replayFolder, "2024-01-02 10.00.00 [Offline] One, Two.WAgame")).ShouldBeFalse();
    }

    [Test]
    public async Task ReturnNonZeroWhenNoReplayMatchesASpecificName()
    {
        using var host = new TestHost();
        var exitCode = await host.Run("delete", "replay", "missing");

        exitCode.ShouldBe(1);
    }

    [Test]
    public async Task ReturnNonZeroWhenMultipleReplaysMatch()
    {
        using var host = new TestHost();
        var replayFolder = host.WormsArmageddon.FindInstallation().ReplayFolder;
        host.WormsArmageddon.WriteReplay("2024-01-02 10.00.00 [Offline] One, Two");
        host.WormsArmageddon.WriteReplay("2024-02-15 12.00.00 [Offline] One, Two");

        var exitCode = await host.Run("delete", "replay", "*");

        exitCode.ShouldBe(1);
        host.FileSystem.File.Exists(Path.Combine(replayFolder, "2024-01-02 10.00.00 [Offline] One, Two.WAgame")).ShouldBeTrue();
        host.FileSystem.File.Exists(Path.Combine(replayFolder, "2024-02-15 12.00.00 [Offline] One, Two.WAgame")).ShouldBeTrue();
    }
}

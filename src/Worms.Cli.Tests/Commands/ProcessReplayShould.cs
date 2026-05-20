using NUnit.Framework;
using Shouldly;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class ProcessReplayShould
{
    [Test]
    public async Task GenerateLogForAReplayWithoutOne()
    {
        using var host = new TestHost();
        var replayFolder = host.WormsArmageddon.FindInstallation().ReplayFolder;
        ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two");

        var exitCode = await host.Run("process", "replay", "2024-01-02");

        exitCode.ShouldBe(0);
        host.FileSystem.File.Exists(Path.Combine(replayFolder, "2024-01-02 10.00.00 [Offline] One, Two.log")).ShouldBeTrue();
    }

    [Test]
    public async Task GenerateLogForEveryMatchingReplay()
    {
        using var host = new TestHost();
        var replayFolder = host.WormsArmageddon.FindInstallation().ReplayFolder;
        ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two");
        ReplayFixtures.WriteReplay(host, "2024-02-15 12.00.00 [Offline] One, Two");

        var exitCode = await host.Run("process", "replay", "*");

        exitCode.ShouldBe(0);
        host.FileSystem.File.Exists(Path.Combine(replayFolder, "2024-01-02 10.00.00 [Offline] One, Two.log")).ShouldBeTrue();
        host.FileSystem.File.Exists(Path.Combine(replayFolder, "2024-02-15 12.00.00 [Offline] One, Two.log")).ShouldBeTrue();
    }

    [Test]
    public async Task ReturnZeroWhenNoMatch()
    {
        using var host = new TestHost();
        var replayFolder = host.WormsArmageddon.FindInstallation().ReplayFolder;

        var exitCode = await host.Run("process", "replay", "missing");

        exitCode.ShouldBe(0);
        host.FileSystem.Directory.GetFiles(replayFolder).ShouldBeEmpty();
    }
}

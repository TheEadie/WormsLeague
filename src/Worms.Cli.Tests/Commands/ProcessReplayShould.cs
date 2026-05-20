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
        var wagame = ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two");
        var fs = host.FileSystem;
        var expectedLog = wagame.Replace(".WAgame", ".log", StringComparison.OrdinalIgnoreCase);

        var exitCode = await host.Run("process", "replay", "2024-01-02");

        exitCode.ShouldBe(0);
        fs.File.Exists(expectedLog).ShouldBeTrue();
    }

    [Test]
    public async Task GenerateLogForEveryMatchingReplay()
    {
        using var host = new TestHost();
        var wagame1 = ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two");
        var wagame2 = ReplayFixtures.WriteReplay(host, "2024-02-15 12.00.00 [Offline] One, Two");
        var fs = host.FileSystem;
        var expectedLog1 = wagame1.Replace(".WAgame", ".log", StringComparison.OrdinalIgnoreCase);
        var expectedLog2 = wagame2.Replace(".WAgame", ".log", StringComparison.OrdinalIgnoreCase);

        var exitCode = await host.Run("process", "replay", "*");

        exitCode.ShouldBe(0);
        fs.File.Exists(expectedLog1).ShouldBeTrue();
        fs.File.Exists(expectedLog2).ShouldBeTrue();
    }

    [Test]
    public async Task ReturnZeroWhenNoMatch()
    {
        using var host = new TestHost();
        var fs = host.FileSystem;

        var exitCode = await host.Run("process", "replay", "missing");

        exitCode.ShouldBe(0);
        fs.Directory.GetFiles(host.WormsArmageddon.FindInstallation().ReplayFolder).ShouldBeEmpty();
    }
}

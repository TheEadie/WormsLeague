using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Cli.Tests.Fakes;

namespace Worms.Cli.Tests.Commands;

[TestFixture]
internal sealed class DeleteReplayShould
{
    [Test]
    public async Task DeleteBothTheWAgameAndItsLog()
    {
        using var host = new TestHost();
        var wagame = ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two", ReplayFixtures.MultiTurnLog);
        var fs = host.Services.GetRequiredService<IFileSystem>();
        var log = wagame.Replace(".WAgame", ".log", StringComparison.OrdinalIgnoreCase);

        var exitCode = await host.Run("delete", "replay", "2024-01-02");

        exitCode.ShouldBe(0);
        fs.File.Exists(wagame).ShouldBeFalse();
        fs.File.Exists(log).ShouldBeFalse();
    }

    [Test]
    public async Task DeleteTheWAgameWhenNoLogPresent()
    {
        using var host = new TestHost();
        var wagame = ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two");
        var fs = host.Services.GetRequiredService<IFileSystem>();

        var exitCode = await host.Run("delete", "replay", "2024-01-02");

        exitCode.ShouldBe(0);
        fs.File.Exists(wagame).ShouldBeFalse();
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
        var wagame1 = ReplayFixtures.WriteReplay(host, "2024-01-02 10.00.00 [Offline] One, Two");
        var wagame2 = ReplayFixtures.WriteReplay(host, "2024-02-15 12.00.00 [Offline] One, Two");
        var fs = host.Services.GetRequiredService<IFileSystem>();

        var exitCode = await host.Run("delete", "replay", "*");

        exitCode.ShouldBe(1);
        fs.File.Exists(wagame1).ShouldBeTrue();
        fs.File.Exists(wagame2).ShouldBeTrue();
    }
}

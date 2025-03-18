using System.Runtime.InteropServices;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Tests.Framework;

namespace Worms.Armageddon.Game.Tests;

[FakeDependencies]
[FakeComponent]
[RealDependencies]
internal sealed class PlayReplayShould(ApiType apiType)
{
    [Test]
    public async Task ErrorWhenNotInstalled()
    {
        var wormsArmageddon = A.WormsArmageddon(apiType).NotInstalled().Build();
        var exception = await Should.ThrowAsync<InvalidOperationException>(wormsArmageddon.PlayReplay("replay.WAGame"));
        exception.Message.ShouldBe("Worms Armageddon is not installed");
    }

    [Test]
    public async Task PlayAReplay()
    {
        var replayFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
        var replayFilePath = Path.Combine(replayFolder, "replay.WAGame");
        var wormsArmageddon = A.WormsArmageddon(apiType).WithReplayFilePath(replayFilePath).Build();

        await wormsArmageddon.PlayReplay(replayFilePath);
    }

    [Test]
    public async Task PlayAReplayStartingAtTime()
    {
        var replayFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
        var replayFilePath = Path.Combine(replayFolder, "replay.WAGame");
        var wormsArmageddon = A.WormsArmageddon(apiType).WithReplayFilePath(replayFilePath).Build();

        await wormsArmageddon.PlayReplay(replayFilePath, new TimeSpan(0, 0, 1, 0));
    }

    [Test]
    public async Task DoNothingWhenReplayDoesntExist()
    {
        var replayFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
        var replayFilePath = Path.Combine(replayFolder, "replay.WAGame");
        var wormsArmageddon = A.WormsArmageddon(apiType).WithReplayFilePath(replayFilePath).Build();

        // Note that with /quiet passed to WA.exe no error is thrown
        await wormsArmageddon.PlayReplay(replayFilePath);
    }
}

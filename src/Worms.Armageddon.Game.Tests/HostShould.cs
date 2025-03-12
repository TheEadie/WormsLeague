using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Tests.Framework;

namespace Worms.Armageddon.Game.Tests;

[FakeDependencies]
[FakeComponent]
[RealDependencies]
internal sealed class HostShould(ApiType apiType)
{
    [Test]
    public async Task LaunchWormsArmageddon()
    {
        var wormsArmageddon = A.WormsArmageddon(apiType).Build();
        await wormsArmageddon.Host();
    }

    [Test]
    public async Task ErrorWhenNotInstalled()
    {
        var wormsArmageddon = A.WormsArmageddon(apiType).NotInstalled().Build();
        var exception = await Should.ThrowAsync<InvalidOperationException>(wormsArmageddon.Host());
        exception.Message.ShouldBe("Worms Armageddon is not installed");
    }

    [Test]
    public async Task SaveReplayFileWhenGameIsFinished()
    {
        var builder = A.WormsArmageddon(apiType);
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        await wormsArmageddon.Host();

        var replayFolder = wormsArmageddon.FindInstallation().ReplayFolder;
        var replayFile = Path.Combine(replayFolder, "replay.WAGame");

        fileSystem.File.Exists(replayFile).ShouldBe(true);
    }
}

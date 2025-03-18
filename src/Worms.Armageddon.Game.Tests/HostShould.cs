using System.Globalization;
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
    public async Task ErrorWhenNotInstalled()
    {
        var wormsArmageddon = A.WormsArmageddon(apiType).NotInstalled().Build();
        var exception = await Should.ThrowAsync<InvalidOperationException>(wormsArmageddon.Host());
        exception.Message.ShouldBe("Worms Armageddon is not installed");
    }

    [Test]
    public async Task LaunchWormsArmageddon()
    {
        var wormsArmageddon = A.WormsArmageddon(apiType).Build();
        await wormsArmageddon.Host();
    }

    [Test]
    public async Task SaveReplayFileWhenGameIsFinished()
    {
        var builder = A.WormsArmageddon(apiType);
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        await wormsArmageddon.Host();

        var replayFolder = wormsArmageddon.FindInstallation().ReplayFolder;
        var replayFiles = fileSystem.Directory.GetFiles(replayFolder, "*.WAGame");
        var dateAsString = DateTime.Now.ToString("yyyy-MM-dd ", CultureInfo.InvariantCulture);
        replayFiles.ShouldContain(x => x.Contains(dateAsString) && x.EndsWith("1-UP, 2-UP.WAGame"));
    }

    [Test]
    public async Task NotSaveReplayFileWhenGameIsNotCompleted()
    {
        var builder = A.WormsArmageddon(apiType).WhereHostCmdDoesNotCreateReplayFile();
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        await wormsArmageddon.Host();

        var replayFolder = wormsArmageddon.FindInstallation().ReplayFolder;
        var replayFiles = fileSystem.Directory.GetFiles(replayFolder, "*.WAGame");
        var dateAsString = DateTime.Now.ToString("yyyy-MM-dd ", CultureInfo.InvariantCulture);
        replayFiles.ShouldNotContain(x => x.Contains(dateAsString) && x.EndsWith("1-UP, 2-UP.WAGame"));
    }
}

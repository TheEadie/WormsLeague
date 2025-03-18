using System.Runtime.InteropServices;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Tests.Framework;

namespace Worms.Armageddon.Game.Tests;

[FakeDependencies]
[FakeComponent]
[RealDependencies]
internal sealed class GenerateReplayLogShould(ApiType apiType)
{
    [Test]
    public async Task ErrorWhenNotInstalled()
    {
        var wormsArmageddon = A.WormsArmageddon(apiType).NotInstalled().Build();
        var exception =
            await Should.ThrowAsync<InvalidOperationException>(wormsArmageddon.GenerateReplayLog("replay.WAGame"));
        exception.Message.ShouldBe("Worms Armageddon is not installed");
    }

    [Test]
    public async Task CreateALogFile()
    {
        var replayFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
        var replayFilePath = Path.Combine(replayFolder, "replay.WAGame");
        var builder = A.WormsArmageddon(apiType).WithReplayFilePath(replayFilePath);
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        await wormsArmageddon.GenerateReplayLog(replayFilePath);

        var replayLogFiles = fileSystem.Directory.GetFiles(replayFolder, "*.log");
        replayLogFiles.ShouldContain(x => x.EndsWith("replay.log"));
    }

    [Test]
    public async Task NotCreateALogFileWhenReplayDoesntExist()
    {
        var replayFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
        var replayFilePath = Path.Combine(replayFolder, "replay.WAGame");
        var builder = A.WormsArmageddon(apiType);
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        // Note that with /quiet passed to WA.exe no error is thrown
        await wormsArmageddon.GenerateReplayLog(replayFilePath);

        var replayLogFiles = fileSystem.Directory.GetFiles(replayFolder, "*.log");
        replayLogFiles.ShouldNotContain(x => x.EndsWith("replay.log"));
    }
}

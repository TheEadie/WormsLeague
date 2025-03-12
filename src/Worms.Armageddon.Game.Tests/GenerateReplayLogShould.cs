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
    public async Task CreateALogFile()
    {
        var builder = A.WormsArmageddon(apiType);
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        await wormsArmageddon.GenerateReplayLog(@"C:\replay.WAGame");

        var replayFolder = wormsArmageddon.FindInstallation().ReplayFolder;
        var replayLogFiles = fileSystem.Directory.GetFiles(replayFolder, "*.log");
        replayLogFiles.ShouldContain(x => x.EndsWith("replay.log"));
    }
}

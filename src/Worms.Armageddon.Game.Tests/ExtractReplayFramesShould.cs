using System.Runtime.InteropServices;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game.Tests.Framework;

namespace Worms.Armageddon.Game.Tests;

[FakeDependencies]
[FakeComponent]
[RealDependencies]
internal sealed class ExtractReplayFramesShould(ApiType apiType)
{
    [Test]
    public async Task ErrorWhenNotInstalled()
    {
        var wormsArmageddon = A.WormsArmageddon(apiType).NotInstalled().Build();
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            wormsArmageddon.ExtractReplayFrames(
                "replay.WAGame",
                10,
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(60)));
        exception.Message.ShouldBe("Worms Armageddon is not installed");
    }

    [Test]
    public async Task ExtractFrames()
    {
        var replayFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
        var replayFilePath = Path.Combine(replayFolder, "replay.WAGame");
        var builder = A.WormsArmageddon(apiType).WithReplayFilePath(replayFilePath);
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        await wormsArmageddon.ExtractReplayFrames(
            replayFilePath,
            10,
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(10));

        var captureFolder = wormsArmageddon.FindInstallation().CaptureFolder;
        var captureFiles = fileSystem.Directory.GetFiles(captureFolder, "*.png");
        // Expect 10 frames per second for 10 seconds
        captureFiles.Count(x => x.Contains("replay", StringComparison.InvariantCulture)).ShouldBe(100);
    }

    [TestCase((uint) 30, 0, 10, 300)]
    [TestCase((uint) 30, 10, 20, 300)]
    [TestCase((uint) 60, 0, 10, 600)]
    [TestCase((uint) 30, 0, 30, 900)]
    public async Task ExtractFramesBasedOnFpsAndDuration(uint fps, int start, int end, int expected)
    {
        var replayFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
        var replayFilePath = Path.Combine(replayFolder, "replay.WAGame");
        var builder = A.WormsArmageddon(apiType).WithReplayFilePath(replayFilePath);
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        await wormsArmageddon.ExtractReplayFrames(
            replayFilePath,
            fps,
            TimeSpan.FromSeconds(start),
            TimeSpan.FromSeconds(end));

        var captureFolder = wormsArmageddon.FindInstallation().CaptureFolder;
        var captureFiles = fileSystem.Directory.GetFiles(captureFolder, "*.png");
        captureFiles.Count(x => x.Contains("replay", StringComparison.InvariantCulture)).ShouldBe(expected);
    }

    [Test]
    public async Task ExtractNoFramesWhenReplayDoesntExist()
    {
        var replayFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
        var replayFilePath = Path.Combine(replayFolder, "replay.WAGame");
        var builder = A.WormsArmageddon(apiType);
        var fileSystem = builder.GetFileSystem();
        var wormsArmageddon = builder.Build();

        await wormsArmageddon.ExtractReplayFrames(
            replayFilePath,
            10,
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(10));

        var captureFolder = wormsArmageddon.FindInstallation().CaptureFolder;
        var captureFiles = fileSystem.Directory.GetFiles(captureFolder, "*.png");
        // Expect 10 frames per second for 10 seconds
        captureFiles.Count(x => x.Contains("replay", StringComparison.InvariantCulture)).ShouldBe(0);
    }
}

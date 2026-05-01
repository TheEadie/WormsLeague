using System.IO.Abstractions.TestingHelpers;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game;

namespace Worms.Armageddon.Gifs.Tests;

internal sealed class GifCreatorShould
{
    [Test]
    public async Task ThrowFrameExtractionFailedWhenFramesFolderMissing()
    {
        var fileSystem = new MockFileSystem();
        const string captureFolder = "/wa/Capture";
        fileSystem.AddDirectory(captureFolder);

        var wormsArmageddon = Substitute.For<IWormsArmageddon>();
        wormsArmageddon.FindInstallation().Returns(GameInfoFor(captureFolder));

        var gifCreator = new GifCreator(wormsArmageddon, fileSystem);

        var exception = await Should.ThrowAsync<GifFrameExtractionFailedException>(
            gifCreator.CreateGif(
                "/storage/replay.gjb",
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(10),
                1,
                "/storage/output"));

        exception.ReplayPath.ShouldBe("/storage/replay.gjb");
        exception.FramesFolder.ShouldBe("/wa/Capture/replay");
    }

    [Test]
    public async Task ThrowFrameExtractionFailedWhenFramesFolderEmpty()
    {
        var fileSystem = new MockFileSystem();
        const string framesFolder = "/wa/Capture/replay";
        fileSystem.AddDirectory(framesFolder);

        var wormsArmageddon = Substitute.For<IWormsArmageddon>();
        wormsArmageddon.FindInstallation().Returns(GameInfoFor("/wa/Capture"));

        var gifCreator = new GifCreator(wormsArmageddon, fileSystem);

        await Should.ThrowAsync<GifFrameExtractionFailedException>(
            gifCreator.CreateGif(
                "/storage/replay.gjb",
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(10),
                1,
                "/storage/output"));
    }

    private static GameInfo GameInfoFor(string captureFolder) =>
        new(
            true,
            "/wa/WA.exe",
            "WA",
            new Version(3, 8),
            "/wa/Schemes",
            "/wa/Replays",
            captureFolder);
}

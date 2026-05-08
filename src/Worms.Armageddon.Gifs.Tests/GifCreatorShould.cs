using System.IO.Abstractions.TestingHelpers;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game;

namespace Worms.Armageddon.Gifs.Tests;

internal sealed class GifCreatorShould
{
    [Test]
    public async Task LookForFramesInExtensionFolderForNonWaGameReplays()
    {
        // WA keeps non-.WAGame extensions when naming the capture folder.
        var fileSystem = new MockFileSystem();
        const string captureFolder = "/wa/Capture";
        fileSystem.AddDirectory(captureFolder);

        var wormsArmageddon = Substitute.For<IWormsArmageddon>();
        wormsArmageddon.FindInstallation().Returns(GameInfoFor(captureFolder));

        var gifCreator = new GifCreator(wormsArmageddon, fileSystem);

        var exception = await Should.ThrowAsync<GifFrameExtractionFailedException>(
            gifCreator.CreateGif(
                "/storage/mo3z4qju.2s4",
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(10),
                1,
                "/storage/output"));

        exception.FramesFolder.ShouldBe("/wa/Capture/mo3z4qju.2s4");
    }

    [Test]
    public async Task StripWaGameExtensionWhenLookingForFrames()
    {
        // WA strips the .WAGame extension when naming the capture folder.
        var fileSystem = new MockFileSystem();
        const string captureFolder = "/wa/Capture";
        fileSystem.AddDirectory(captureFolder);

        var wormsArmageddon = Substitute.For<IWormsArmageddon>();
        wormsArmageddon.FindInstallation().Returns(GameInfoFor(captureFolder));

        var gifCreator = new GifCreator(wormsArmageddon, fileSystem);

        var exception = await Should.ThrowAsync<GifFrameExtractionFailedException>(
            gifCreator.CreateGif(
                "/storage/sample.WAGame",
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(10),
                1,
                "/storage/output"));

        exception.FramesFolder.ShouldBe("/wa/Capture/sample");
    }

    [Test]
    public async Task ThrowFrameExtractionFailedWhenFramesFolderEmpty()
    {
        var fileSystem = new MockFileSystem();
        const string framesFolder = "/wa/Capture/replay.gjb";
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

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using ImageMagick;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game;

namespace Worms.Armageddon.Gifs.Tests;

internal sealed class GifCreatorShould
{
    private string _tempRoot = null!;

    [SetUp]
    public void SetUp()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"GifCreatorTest_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(_tempRoot);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Test]
    public async Task ProduceGifFromExtractedFramesForWaGameReplay()
    {
        // WA strips the .WAGame extension when naming the capture folder.
        var (captureFolder, outputFolder) = MakeWorkingFolders();
        var wormsArmageddon = StubArmageddon(captureFolder, framesFolderName: "sample");

        var gifCreator = new GifCreator(wormsArmageddon, new FileSystem());

        var gifFileName = await gifCreator.CreateGif(
            "/storage/sample.WAGame",
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(10),
            5,
            outputFolder);

        gifFileName.ShouldBe("sample - 5.gif");
        File.Exists(Path.Combine(outputFolder, gifFileName)).ShouldBeTrue();
    }

    [Test]
    public async Task ProduceGifFromExtractedFramesForNonWaGameReplay()
    {
        // WA keeps non-.WAGame extensions when naming the capture folder.
        var (captureFolder, outputFolder) = MakeWorkingFolders();
        var wormsArmageddon = StubArmageddon(captureFolder, framesFolderName: "mo3z4qju.2s4");

        var gifCreator = new GifCreator(wormsArmageddon, new FileSystem());

        var gifFileName = await gifCreator.CreateGif(
            "/storage/mo3z4qju.2s4",
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(10),
            5,
            outputFolder);

        gifFileName.ShouldBe("mo3z4qju - 5.gif");
        File.Exists(Path.Combine(outputFolder, gifFileName)).ShouldBeTrue();
    }

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
        exception.FramesFolder.ShouldBe("/wa/Capture/replay.gjb");
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

        var exception = await Should.ThrowAsync<GifFrameExtractionFailedException>(
            gifCreator.CreateGif(
                "/storage/replay.gjb",
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(10),
                1,
                "/storage/output"));

        exception.ReplayPath.ShouldBe("/storage/replay.gjb");
        exception.FramesFolder.ShouldBe("/wa/Capture/replay.gjb");
    }

    private (string CaptureFolder, string OutputFolder) MakeWorkingFolders()
    {
        var captureFolder = Path.Combine(_tempRoot, "Capture");
        var outputFolder = Path.Combine(_tempRoot, "output");
        _ = Directory.CreateDirectory(captureFolder);
        _ = Directory.CreateDirectory(outputFolder);
        return (captureFolder, outputFolder);
    }

    private static IWormsArmageddon StubArmageddon(string captureFolder, string framesFolderName)
    {
        var wormsArmageddon = Substitute.For<IWormsArmageddon>();
        wormsArmageddon.FindInstallation().Returns(GameInfoFor(captureFolder));
        _ = wormsArmageddon
            .ExtractReplayFrames(default!, default, default, default, default, default)
            .ReturnsForAnyArgs(async call =>
            {
                _ = call;
                var framesFolder = Path.Combine(captureFolder, framesFolderName);
                _ = Directory.CreateDirectory(framesFolder);
                for (var i = 0; i < 3; i++)
                {
                    using var image = new MagickImage(MagickColors.Black, 16, 16);
                    await image.WriteAsync(Path.Combine(framesFolder, $"video_{i:D3}.png"));
                }
            });
        return wormsArmageddon;
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

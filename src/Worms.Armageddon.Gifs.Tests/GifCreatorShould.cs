using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Game;
using Worms.Armageddon.Game.Fake;

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
        var (gifCreator, fileSystem, gamePath) = SetupFakeWa();
        var replayPath = Path.Combine(_tempRoot, "sample.WAGame");
        await fileSystem.File.WriteAllBytesAsync(replayPath, []);
        var outputFolder = Path.Combine(_tempRoot, "output");
        _ = Directory.CreateDirectory(outputFolder);

        var gifFileName = await gifCreator.CreateGif(
            replayPath,
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(2),
            5,
            outputFolder);

        gifFileName.ShouldBe("sample - 5.gif");
        File.Exists(Path.Combine(outputFolder, gifFileName)).ShouldBeTrue();
        Directory.Exists(Path.Combine(gamePath, "User", "Capture", "sample")).ShouldBeFalse(
            "GifCreator should clean up frames after producing the GIF");
    }

    [Test]
    public async Task ProduceGifFromExtractedFramesForNonWaGameReplay()
    {
        // WA keeps non-.WAGame extensions when naming the capture folder.
        var (gifCreator, fileSystem, gamePath) = SetupFakeWa();
        var replayPath = Path.Combine(_tempRoot, "mo3z4qju.2s4");
        await fileSystem.File.WriteAllBytesAsync(replayPath, []);
        var outputFolder = Path.Combine(_tempRoot, "output");
        _ = Directory.CreateDirectory(outputFolder);

        var gifFileName = await gifCreator.CreateGif(
            replayPath,
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(2),
            5,
            outputFolder);

        gifFileName.ShouldBe("mo3z4qju - 5.gif");
        File.Exists(Path.Combine(outputFolder, gifFileName)).ShouldBeTrue();
        Directory.Exists(Path.Combine(gamePath, "User", "Capture", "mo3z4qju.2s4")).ShouldBeFalse(
            "GifCreator should clean up frames after producing the GIF");
    }

    [Test]
    public async Task ThrowFrameExtractionFailedWhenFramesFolderMissing()
    {
        // No replay file on disk, so the Fake skips frame extraction and the folder never appears.
        var fileSystem = new MockFileSystem();
        var gifCreator = BuildGifCreatorWithFakeWa(fileSystem, gamePath: "/wa");

        var exception = await Should.ThrowAsync<GifFrameExtractionFailedException>(
            gifCreator.CreateGif(
                "/storage/replay.gjb",
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(10),
                1,
                "/storage/output"));

        exception.ReplayPath.ShouldBe("/storage/replay.gjb");
        exception.FramesFolder.ShouldBe("/wa/User/Capture/replay.gjb");
    }

    [Test]
    public async Task ThrowFrameExtractionFailedWhenFramesFolderEmpty()
    {
        var fileSystem = new MockFileSystem();
        // No-op frame content + zero-length window => folder created but no frames written.
        var gifCreator = BuildGifCreatorWithFakeWa(fileSystem, gamePath: "/wa");
        fileSystem.AddFile("/storage/replay.gjb", new MockFileData([]));

        var exception = await Should.ThrowAsync<GifFrameExtractionFailedException>(
            gifCreator.CreateGif(
                "/storage/replay.gjb",
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(0),
                1,
                "/storage/output"));

        exception.ReplayPath.ShouldBe("/storage/replay.gjb");
        exception.FramesFolder.ShouldBe("/wa/User/Capture/replay.gjb");
    }

    private (GifCreator GifCreator, IFileSystem FileSystem, string GamePath) SetupFakeWa()
    {
        var fileSystem = new FileSystem();
        var gamePath = Path.Combine(_tempRoot, "wa");
        var gifCreator = BuildGifCreatorWithFakeWa(fileSystem, gamePath, frameContent: _ => TinyPng);
        return (gifCreator, fileSystem, gamePath);
    }

    private static GifCreator BuildGifCreatorWithFakeWa(
        IFileSystem fileSystem,
        string gamePath,
        Func<int, byte[]>? frameContent = null)
    {
        var services = new ServiceCollection()
            .AddFakeInstalledWormsArmageddonServices(fileSystem, gamePath, frameContent: frameContent)
            .BuildServiceProvider();

        var wormsArmageddon = services.GetRequiredService<IWormsArmageddon>();
        return new GifCreator(wormsArmageddon, fileSystem);
    }

    private static readonly byte[] TinyPng = BuildTinyPng();

    private static byte[] BuildTinyPng()
    {
        using var image = new MagickImage(MagickColors.Black, 16, 16);
        return image.ToByteArray(MagickFormat.Png);
    }
}

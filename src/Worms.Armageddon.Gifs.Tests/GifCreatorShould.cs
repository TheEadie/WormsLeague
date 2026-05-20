using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
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

    [Test]
    public async Task AssembleFramesInSortedOrderWhenFileSystemReturnsThemUnsorted()
    {
        // Directory.GetFiles makes no ordering guarantee — on Linux/ext4 it can return files
        // in arbitrary order. This test simulates that by reversing the GetFiles result, and
        // verifies the resulting GIF still plays frames in sorted (filename) order.
        var gamePath = Path.Combine(_tempRoot, "wa");
        var captureFolder = Path.Combine(gamePath, "User", "Capture", "sample");
        _ = Directory.CreateDirectory(captureFolder);
        var outputFolder = Path.Combine(_tempRoot, "output");
        _ = Directory.CreateDirectory(outputFolder);
        var replayPath = Path.Combine(_tempRoot, "sample.WAGame");
        await File.WriteAllBytesAsync(replayPath, []);

        var frameColors = new[] { MagickColors.Red, MagickColors.Lime, MagickColors.Blue };
        var wa = Substitute.For<IWormsArmageddon>();
        _ = wa.FindInstallation()
            .Returns(
                new GameInfo(
                    true,
                    Path.Combine(gamePath, "WA.exe"),
                    "WA",
                    new Version(1, 0, 0, 0),
                    Path.Combine(gamePath, "User", "Schemes"),
                    Path.Combine(gamePath, "User", "Games"),
                    Path.Combine(gamePath, "User", "Capture")));
        _ = wa.ExtractReplayFrames(
                Arg.Any<string>(),
                Arg.Any<uint>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<int>(),
                Arg.Any<int>())
            .Returns(Task.CompletedTask)
            .AndDoes(_ => WriteColoredFrames(captureFolder, frameColors));

        var fileSystem = BuildFileSystemWithReversedGetFiles();
        var gifCreator = new GifCreator(wa, fileSystem);

        var gifFileName = await gifCreator.CreateGif(
            replayPath,
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(1),
            1,
            outputFolder);

        using var gif = new MagickImageCollection(Path.Combine(outputFolder, gifFileName));
        gif.Count.ShouldBe(frameColors.Length);
        for (var i = 0; i < frameColors.Length; i++)
        {
            var pixel = gif[i].GetPixels().GetPixel(0, 0).ToColor()!;
            pixel.R.ShouldBe(frameColors[i].R);
            pixel.G.ShouldBe(frameColors[i].G);
            pixel.B.ShouldBe(frameColors[i].B);
        }
    }

    private static void WriteColoredFrames(string folder, MagickColor[] colors)
    {
        _ = Directory.CreateDirectory(folder);
        for (var i = 0; i < colors.Length; i++)
        {
            using var image = new MagickImage(colors[i], 16, 16);
            image.Write(Path.Combine(folder, $"frame_{i:D3}.png"));
        }
    }

    private static IFileSystem BuildFileSystemWithReversedGetFiles()
    {
        var real = new FileSystem();
        var fileSystem = Substitute.For<IFileSystem>();
        _ = fileSystem.Path.Returns(real.Path);
        _ = fileSystem.File.Returns(real.File);

        var directory = Substitute.For<IDirectory>();
        _ = directory.Exists(Arg.Any<string>())
            .Returns(c => real.Directory.Exists(c.ArgAt<string>(0)));
        _ = directory.CreateDirectory(Arg.Any<string>())
            .Returns(c => real.Directory.CreateDirectory(c.ArgAt<string>(0)));
        directory.When(d => d.Delete(Arg.Any<string>(), Arg.Any<bool>()))
            .Do(c => real.Directory.Delete(c.ArgAt<string>(0), c.ArgAt<bool>(1)));
        _ = directory.GetFiles(Arg.Any<string>(), Arg.Any<string>())
            .Returns(
                c => real.Directory
                    .GetFiles(c.ArgAt<string>(0), c.ArgAt<string>(1))
                    .OrderByDescending(f => f, StringComparer.Ordinal)
                    .ToArray());
        _ = fileSystem.Directory.Returns(directory);

        return fileSystem;
    }

    private (GifCreator GifCreator, IFileSystem FileSystem, string GamePath) SetupFakeWa()
    {
        var fileSystem = new FileSystem();
        var gamePath = Path.Combine(_tempRoot, "wa");
        return (BuildGifCreatorWithFakeWa(fileSystem, gamePath), fileSystem, gamePath);
    }

    private static GifCreator BuildGifCreatorWithFakeWa(IFileSystem fileSystem, string gamePath)
    {
        var services = new ServiceCollection()
            .AddFakeInstalledWormsArmageddonServices(fileSystem, gamePath)
            .BuildServiceProvider();

        var wormsArmageddon = services.GetRequiredService<IWormsArmageddon>();
        return new GifCreator(wormsArmageddon, fileSystem);
    }
}

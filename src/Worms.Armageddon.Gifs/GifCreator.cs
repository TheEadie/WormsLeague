using System.IO.Abstractions;
using ImageMagick;
using JetBrains.Annotations;
using Worms.Armageddon.Game;

namespace Worms.Armageddon.Gifs;

[PublicAPI]
public sealed class GifCreator(IWormsArmageddon wormsArmageddon, IFileSystem fileSystem)
{
    public async Task<string> CreateGif(
        string replayPath,
        TimeSpan turnStart,
        TimeSpan turnEnd,
        int turnNumber,
        string outputFolder,
        uint framesPerSecond = 5,
        uint speedMultiplier = 2,
        TimeSpan? startOffset = null,
        TimeSpan? endOffset = null)
    {
        var replayName = fileSystem.Path.GetFileNameWithoutExtension(replayPath);
        var worms = wormsArmageddon.FindInstallation();
        var framesFolder = fileSystem.Path.Combine(worms.CaptureFolder, replayName);
        var gifFileName = $"{replayName} - {turnNumber}.gif";
        var outputFilePath = fileSystem.Path.Combine(outputFolder, gifFileName);

        var animationDelay = 100 / framesPerSecond / speedMultiplier;
        var start = turnStart + (startOffset ?? TimeSpan.Zero);
        var end = turnEnd - (endOffset ?? TimeSpan.Zero);

        DeleteFrames(framesFolder);
        await wormsArmageddon.ExtractReplayFrames(replayPath, framesPerSecond, start, end);
        CreateGifFromFiles(framesFolder, outputFilePath, animationDelay, 640, 480);
        DeleteFrames(framesFolder);

        return gifFileName;
    }

    private void DeleteFrames(string framesFolder)
    {
        if (fileSystem.Directory.Exists(framesFolder))
        {
            fileSystem.Directory.Delete(framesFolder, true);
        }
    }

    private void CreateGifFromFiles(
        string framesFolder,
        string outputFile,
        uint animationDelay,
        uint width,
        uint height)
    {
        var frames = fileSystem.Directory.GetFiles(framesFolder, "*.png");

        using var collection = new MagickImageCollection();
        foreach (var file in frames)
        {
            var image = new MagickImage(file);
            image.Resize(width, height);
            image.AnimationDelay = animationDelay;
            collection.Add(image);
        }

        var settings = new QuantizeSettings { Colors = 256 };

        _ = collection.Quantize(settings);
        collection.OptimizeTransparency();
        collection.Write(outputFile);
    }
}

using JetBrains.Annotations;

namespace Worms.Armageddon.Gifs;

[PublicAPI]
public sealed class GifFrameExtractionFailedException : Exception
{
    public GifFrameExtractionFailedException()
    {
    }

    public GifFrameExtractionFailedException(string message)
        : base(message)
    {
    }

    public GifFrameExtractionFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public GifFrameExtractionFailedException(string replayPath, string framesFolder)
        : base(BuildMessage(replayPath, framesFolder))
    {
        ReplayPath = replayPath;
        FramesFolder = framesFolder;
    }

    public string? ReplayPath { get; }

    public string? FramesFolder { get; }

    private static string BuildMessage(string replayPath, string framesFolder) =>
        $"Worms Armageddon did not produce any frames in '{framesFolder}' for replay '{replayPath}'. "
        + "Check the wa-runner logs for wine/WA stderr.";
}

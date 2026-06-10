using JetBrains.Annotations;

namespace Worms.Armageddon.Gifs;

/// <summary>
/// Assembles a GIF animation for a turn of a replay. The implementation drives WA frame extraction and ImageMagick,
/// so callers depend on this abstraction and fake it in the unit tier; the real assembly is exercised by the
/// Armageddon Gifs integration tests.
/// </summary>
[PublicAPI]
public interface IGifCreator
{
    /// <summary>
    /// Extracts frames for the given time window of a replay and assembles them into a GIF in
    /// <paramref name="outputFolder"/>. Returns the generated file name (not the full path).
    /// </summary>
    Task<string> CreateGif(
        string replayPath,
        TimeSpan turnStart,
        TimeSpan turnEnd,
        int turnNumber,
        string outputFolder,
        uint framesPerSecond = 5,
        uint speedMultiplier = 2,
        TimeSpan? startOffset = null,
        TimeSpan? endOffset = null);
}

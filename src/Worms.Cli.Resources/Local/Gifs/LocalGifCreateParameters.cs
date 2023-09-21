using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Resources.Local.Gifs;

public record LocalGifCreateParameters(
    LocalReplay Replay,
    uint Turn,
    TimeSpan StartOffset,
    TimeSpan EndOffset,
    uint FramesPerSecond,
    uint SpeedMultiplier);

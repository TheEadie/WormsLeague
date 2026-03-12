using JetBrains.Annotations;

namespace Worms.Cli.Resources.Local.Replays;

[PublicAPI]
public record ReplayPaths(string WAgamePath, string? LogPath);

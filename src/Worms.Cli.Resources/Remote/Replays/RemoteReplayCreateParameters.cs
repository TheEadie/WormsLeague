using JetBrains.Annotations;

namespace Worms.Cli.Resources.Remote.Replays;

[PublicAPI]
public record RemoteReplayCreateParameters(string Name, string FilePath);

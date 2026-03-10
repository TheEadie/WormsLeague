using JetBrains.Annotations;

namespace Worms.Cli.Resources.Remote.Replays;

[PublicAPI]
public record RemoteReplay(string Id, string Name, string Status);

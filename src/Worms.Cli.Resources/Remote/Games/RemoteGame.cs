using JetBrains.Annotations;

namespace Worms.Cli.Resources.Remote.Games;

[PublicAPI]
public record RemoteGame(string Id, string Status, string HostMachine);

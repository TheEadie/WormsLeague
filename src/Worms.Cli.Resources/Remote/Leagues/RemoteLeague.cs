using JetBrains.Annotations;

namespace Worms.Cli.Resources.Remote.Leagues;

[PublicAPI]
public record RemoteLeague(string Name, Version Version);

using JetBrains.Annotations;

namespace Worms.Cli.Resources.Remote.Games;

[PublicAPI]
public interface IRemoteGameUpdater
{
    Task SetGameComplete(RemoteGame game, CancellationToken cancellationToken);
}

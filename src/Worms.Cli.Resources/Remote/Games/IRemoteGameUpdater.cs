namespace Worms.Cli.Resources.Remote.Games;

public interface IRemoteGameUpdater
{
    Task SetGameComplete(RemoteGame game, CancellationToken cancellationToken);
}

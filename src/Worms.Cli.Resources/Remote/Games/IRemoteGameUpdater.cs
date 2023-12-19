using Serilog;

namespace Worms.Cli.Resources.Remote.Games;

public interface IRemoteGameUpdater
{
    Task SetGameComplete(RemoteGame game, ILogger logger, CancellationToken cancellationToken);
}

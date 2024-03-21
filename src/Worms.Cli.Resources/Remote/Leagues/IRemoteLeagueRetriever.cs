namespace Worms.Cli.Resources.Remote.Leagues;

public interface IRemoteLeagueRetriever
{
    Task<RemoteLeague> Retrieve(string id);
}

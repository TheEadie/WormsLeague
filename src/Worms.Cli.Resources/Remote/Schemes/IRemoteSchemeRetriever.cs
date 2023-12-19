namespace Worms.Cli.Resources.Remote.Schemes;

public interface IRemoteSchemeRetriever
{
    Task<RemoteScheme> Retrieve(string id);
}

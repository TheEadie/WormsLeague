namespace Worms.Cli.Resources.Remote.Updates;

public interface ICliUpdateRetriever
{
    public Task<Version> GetLatestCliVersion();
}

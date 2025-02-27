namespace Worms.Cli.Resources.Remote.Updates;

public interface ICliUpdateRetriever
{
    Task<Version> GetLatestCliVersion();
}

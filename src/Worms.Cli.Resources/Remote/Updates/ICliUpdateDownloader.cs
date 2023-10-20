namespace Worms.Cli.Resources.Remote.Updates;

public interface ICliUpdateDownloader
{
    Task DownloadLatestCli(string updateFolder);
}

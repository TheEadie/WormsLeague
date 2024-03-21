namespace Worms.Cli.Resources.Remote.Schemes;

public interface IRemoteSchemeDownloader
{
    Task Download(string id, string destinationFilename, string destinationFolder);
}

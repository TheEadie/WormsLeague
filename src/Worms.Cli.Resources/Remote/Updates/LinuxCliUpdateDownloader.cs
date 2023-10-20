namespace Worms.Cli.Resources.Remote.Updates;

internal sealed class LinuxCliUpdateDownloader : ICliUpdateDownloader
{
    private readonly IWormsServerApi _api;

    public LinuxCliUpdateDownloader(IWormsServerApi api) => _api = api;

    public async Task DownloadLatestCli(string updateFolder)
    {
        var bytes = await _api.DownloadLatestCli("linux");
        await File.WriteAllBytesAsync(Path.Combine(updateFolder, "update.tar.gz"), bytes);
        // TODO: Unzip
    }
}

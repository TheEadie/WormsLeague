namespace Worms.Cli.Resources.Remote.Updates;

internal sealed class WindowsCliUpdateDownloader : ICliUpdateDownloader
{
    private readonly IWormsServerApi _api;

    public WindowsCliUpdateDownloader(IWormsServerApi api) => _api = api;

    public async Task DownloadLatestCli(string updateFolder)
    {
        var bytes = await _api.DownloadLatestCli("windows");
        await File.WriteAllBytesAsync(Path.Combine(updateFolder, "update.zip"), bytes);
        // TODO: Unzip
    }
}

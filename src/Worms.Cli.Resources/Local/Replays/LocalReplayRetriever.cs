using System.IO.Abstractions;
using Worms.Armageddon.Files.Replays;
using Worms.Armageddon.Files.Replays.Filename;
using Worms.Armageddon.Files.Replays.Text;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayRetriever(
    ILocalReplayLocator localReplayLocator,
    IReplayFilenameParser replayFilenameParser,
    IFileSystem fileSystem,
    IReplayTextReader replayTextReader) : IResourceRetriever<LocalReplay>
{
    public Task<IReadOnlyCollection<LocalReplay>> Retrieve(CancellationToken cancellationToken) =>
        Retrieve("*", cancellationToken);

    public async Task<IReadOnlyCollection<LocalReplay>> Retrieve(string pattern, CancellationToken cancellationToken)
    {
        var resources = new List<LocalReplay>();

        foreach (var paths in localReplayLocator.GetReplayPaths(pattern))
        {
            var replayDetailsFromFilename = replayFilenameParser.Parse(paths.WAgamePath);
            if (fileSystem.File.Exists(paths.LogPath))
            {
                var content = await fileSystem.File.ReadAllTextAsync(paths.LogPath, cancellationToken);
                resources.Add(
                    new LocalReplay(
                        paths,
                        replayTextReader.GetModel(content),
                        replayDetailsFromFilename.HostMachineName == replayDetailsFromFilename.LocalMachineName));
            }
            else
            {
                resources.Add(
                    new LocalReplay(
                        paths,
                        new ReplayResource(replayDetailsFromFilename.Date, false, [], string.Empty, [], string.Empty),
                        replayDetailsFromFilename.HostMachineName == replayDetailsFromFilename.LocalMachineName));
            }
        }

        return [.. resources.OrderByDescending(x => x.Details.Date)];
    }
}

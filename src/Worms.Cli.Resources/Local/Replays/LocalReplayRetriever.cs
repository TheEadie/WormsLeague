using System.IO.Abstractions;
using Worms.Armageddon.Files.Replays;
using Worms.Armageddon.Files.Replays.Text;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayRetriever(
    ILocalReplayLocator localReplayLocator,
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
            if (fileSystem.File.Exists(paths.LogPath))
            {
                var content = await fileSystem.File.ReadAllTextAsync(paths.LogPath, cancellationToken)
                    .ConfigureAwait(false);
                resources.Add(new LocalReplay(paths, replayTextReader.GetModel(content)));
            }
            else
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(paths.WAgamePath);
                var startIndex = fileName.IndexOf('[', StringComparison.InvariantCulture);
                var dateString = fileName[..(startIndex - 1)];
                var date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH.mm.ss", null);
                resources.Add(
                    new LocalReplay(paths, new ReplayResource(date, false, [], string.Empty, [], string.Empty)));
            }
        }

        return [.. resources.OrderByDescending(x => x.Details.Date)];
    }
}

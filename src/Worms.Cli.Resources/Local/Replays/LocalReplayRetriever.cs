using System.IO.Abstractions;
using Serilog;
using Worms.Armageddon.Files.Replays;
using Worms.Armageddon.Files.Replays.Text;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayRetriever(
    ILocalReplayLocator localReplayLocator,
    IFileSystem fileSystem,
    IReplayTextReader replayTextReader) : IResourceRetriever<LocalReplay>
{
    public Task<IReadOnlyCollection<LocalReplay>> Retrieve(ILogger logger, CancellationToken cancellationToken) =>
        Retrieve("*", logger, cancellationToken);

    public async Task<IReadOnlyCollection<LocalReplay>> Retrieve(
        string pattern,
        ILogger logger,
        CancellationToken cancellationToken)
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
                    new LocalReplay(
                        paths,
                        new ReplayResource(
                            date,
                            false,
                            new List<Team>(0),
                            string.Empty,
                            new List<Turn>(0),
                            string.Empty)));
            }
        }

        return resources.OrderByDescending(x => x.Details.Date).ToList();
    }
}

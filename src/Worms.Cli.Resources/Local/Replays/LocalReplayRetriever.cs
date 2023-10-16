using System.IO.Abstractions;
using Serilog;
using Worms.Armageddon.Files.Replays;
using Worms.Armageddon.Files.Replays.Text;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayRetriever : IResourceRetriever<LocalReplay>
{
    private readonly ILocalReplayLocator _localReplayLocator;
    private readonly IFileSystem _fileSystem;
    private readonly IReplayTextReader _replayTextReader;

    public LocalReplayRetriever(
        ILocalReplayLocator localReplayLocator,
        IFileSystem fileSystem,
        IReplayTextReader replayTextReader)
    {
        _localReplayLocator = localReplayLocator;
        _fileSystem = fileSystem;
        _replayTextReader = replayTextReader;
    }

    public Task<IReadOnlyCollection<LocalReplay>> Retrieve(ILogger logger, CancellationToken cancellationToken) =>
        Retrieve("*", logger, cancellationToken);

    public Task<IReadOnlyCollection<LocalReplay>> Retrieve(
        string pattern,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var resources = new List<LocalReplay>();

        foreach (var paths in _localReplayLocator.GetReplayPaths(pattern))
        {
            if (_fileSystem.File.Exists(paths.LogPath))
            {
                resources.Add(
                    new LocalReplay(paths, _replayTextReader.GetModel(_fileSystem.File.ReadAllText(paths.LogPath))));
            }
            else
            {
                var fileName = _fileSystem.Path.GetFileNameWithoutExtension(paths.WAgamePath);
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

        return Task.FromResult<IReadOnlyCollection<LocalReplay>>(
            resources.OrderByDescending(x => x.Details.Date).ToList());
    }
}

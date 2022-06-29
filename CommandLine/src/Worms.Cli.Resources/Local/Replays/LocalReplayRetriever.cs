using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Worms.Armageddon.Resources.Replays;
using Worms.Armageddon.Resources.Replays.Text;

namespace Worms.Cli.Resources.Local.Replays
{
    internal class LocalReplayRetriever : IResourceRetriever<LocalReplay>
    {
        private readonly ILocalReplayLocator _localReplayLocator;
        private readonly IFileSystem _fileSystem;
        private readonly IReplayTextReader _replayTextReader;

        public LocalReplayRetriever(ILocalReplayLocator localReplayLocator, IFileSystem fileSystem, IReplayTextReader replayTextReader)
        {
            _localReplayLocator = localReplayLocator;
            _fileSystem = fileSystem;
            _replayTextReader = replayTextReader;
        }

        public Task<IReadOnlyCollection<LocalReplay>> Get(ILogger logger, CancellationToken cancellationToken)
            => Get("*", logger, cancellationToken);
        
        public Task<IReadOnlyCollection<LocalReplay>> Get(string pattern, ILogger logger, CancellationToken cancellationToken)
        {
            var resources = new List<LocalReplay>();

            foreach (var paths in _localReplayLocator.GetReplayPaths(pattern))
            {
                if (_fileSystem.File.Exists(paths.LogPath))
                {
                    resources.Add(new LocalReplay(
                        paths,
                        _replayTextReader.GetModel(_fileSystem.File.ReadAllText(paths.LogPath))));
                }
                else
                {
                    var fileName = _fileSystem.Path.GetFileNameWithoutExtension(paths.WAgamePath);
                    var startIndex = fileName.IndexOf('[');
                    var dateString = fileName.Substring(0, startIndex - 1);
                    var date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH.mm.ss", null);
                    resources.Add(
                        new LocalReplay(paths,
                        new ReplayResource(
                            date,
                            false,
                            new List<Team>(0),
                            null,
                            new List<Turn>(0),
                            string.Empty)
                        ));
                }
            }

            return Task.FromResult<IReadOnlyCollection<LocalReplay>>(resources.OrderByDescending(x => x.Details.Date).ToList());
        }
    }
}

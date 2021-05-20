using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Worms.Armageddon.Resources.Replays;
using Worms.Armageddon.Resources.Replays.Text;

namespace Worms.Cli.Resources.Replays
{
    internal class LocalReplayRetriever : IResourceRetriever<LocalReplay>
    {
        private readonly IReplayLocator _replayLocator;
        private readonly IFileSystem _fileSystem;
        private readonly IReplayTextReader _replayTextReader;

        public LocalReplayRetriever(IReplayLocator replayLocator, IFileSystem fileSystem, IReplayTextReader replayTextReader)
        {
            _replayLocator = replayLocator;
            _fileSystem = fileSystem;
            _replayTextReader = replayTextReader;
        }

        public IReadOnlyCollection<LocalReplay> Get(string pattern = "*")
        {
            var resources = new List<LocalReplay>();

            foreach (var paths in _replayLocator.GetReplayPaths(pattern))
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
                            "local",
                            false,
                            new List<Team>(0),
                            null,
                            new List<Turn>(0),
                            string.Empty)
                        ));
                }
            }

            return resources.OrderByDescending(x => x.Details.Date).ToList();
        }
    }
}

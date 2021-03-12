using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Worms.Armageddon.Game.Replays;
using Worms.Armageddon.Resources.Replays;
using Worms.Armageddon.Resources.Replays.Text;
using Worms.Resources.Replays.Text;

namespace Worms.Resources.Replays
{
    internal class LocalReplayRetriever : IResourceRetriever<ReplayResource>
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

        public IReadOnlyCollection<ReplayResource> Get(string pattern = "*")
        {
            var resources = new List<ReplayResource>();

            foreach (var paths in _replayLocator.GetReplayPaths(pattern))
            {
                if (_fileSystem.File.Exists(paths.LogPath))
                {
                    resources.Add(_replayTextReader.GetModel(_fileSystem.File.ReadAllText(paths.LogPath)));
                }
                else
                {
                    var fileName = _fileSystem.Path.GetFileNameWithoutExtension(paths.WAgamePath);
                    var startIndex = fileName.IndexOf('[');
                    var dateString = fileName.Substring(0, startIndex - 1);
                    var date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH.mm.ss", null);
                    resources.Add(
                        new ReplayResource(
                            date,
                            "local",
                            false,
                            new List<string>(),
                            string.Empty,
                            string.Empty)
                        );
                }
            }

            return resources.OrderByDescending(x => x.Date).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Worms.Resources.Games.Text;
using Worms.WormsArmageddon.Replays;

namespace Worms.Resources.Games
{
    internal class LocalGameRetriever : IResourceRetriever<GameResource>
    {
        private readonly IReplayLocator _replayLocator;
        private readonly IFileSystem _fileSystem;
        private readonly IGameTextReader _gameTextReader;

        public LocalGameRetriever(IReplayLocator replayLocator, IFileSystem fileSystem, IGameTextReader gameTextReader)
        {
            _replayLocator = replayLocator;
            _fileSystem = fileSystem;
            _gameTextReader = gameTextReader;
        }

        public IReadOnlyCollection<GameResource> Get(string pattern = "*")
        {
            var resources = new List<GameResource>();

            foreach (var game in _replayLocator.GetReplayPaths(pattern))
            {
                var fileName = _fileSystem.Path.GetFileNameWithoutExtension(game);
                var folder = _fileSystem.Path.GetDirectoryName(game);
                var replayLogFilePath = _fileSystem.Path.Combine(folder, fileName + ".log");

                if (_fileSystem.File.Exists(replayLogFilePath))
                {
                    resources.Add(_gameTextReader.GetModel(_fileSystem.File.ReadAllText(replayLogFilePath)));
                }
                else
                {
                    var startIndex = fileName.IndexOf('[');
                    var dateString = fileName.Substring(0, startIndex - 1);
                    var date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH.mm.ss", null);
                    resources.Add(
                        new GameResource(
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

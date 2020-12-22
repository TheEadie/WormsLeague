using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Worms.Resources.Games.Text;
using Worms.WormsArmageddon;

namespace Worms.Resources.Games
{
    internal class LocalGameRetriever : IResourceRetriever<GameResource>
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;
        private readonly IGameTextReader _gameTextReader;

        public LocalGameRetriever(IWormsLocator wormsLocator, IFileSystem fileSystem, IGameTextReader gameTextReader)
        {
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
            _gameTextReader = gameTextReader;
        }

        public IReadOnlyCollection<GameResource> Get(string pattern = "*")
        {
            var gameInfo = _wormsLocator.Find();

            if (!gameInfo.IsInstalled)
            {
                return new List<GameResource>(0);
            }

            var resources = new List<GameResource>();

            foreach (var game in _fileSystem.Directory.GetFiles(gameInfo.GamesFolder, $"{pattern}.WAgame"))
            {
                var fileName = _fileSystem.Path.GetFileNameWithoutExtension(game);
                var replayLogFilePath = _fileSystem.Path.Combine(gameInfo.GamesFolder, fileName + ".log");

                if (_fileSystem.File.Exists(replayLogFilePath))
                {
                    resources.Add(_gameTextReader.GetModel(_fileSystem.File.ReadAllText(replayLogFilePath)));
                }
                else
                {
                    var startIndex = fileName.IndexOf('[');
                    var dateString = fileName.Substring(0, startIndex - 1);
                    var date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH.mm.ss", null);
                    resources.Add(new GameResource(date, "local", false, new List<string>()));
                }
            }

            return resources.OrderByDescending(x => x.Date).ToList();
        }
    }
}

using System.Collections.Generic;
using System.IO.Abstractions;
using Worms.WormsArmageddon;

namespace Worms.Resources.Games
{
    internal class LocalGameRetriever : IGameRetriever
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;

        public LocalGameRetriever(IWormsLocator wormsLocator, IFileSystem fileSystem)
        {
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
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
                resources.Add(new GameResource(fileName, "local"));
            }

            return resources;
        }
    }
}

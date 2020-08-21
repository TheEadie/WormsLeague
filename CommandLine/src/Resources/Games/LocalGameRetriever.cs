using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Worms.WormsArmageddon;

namespace Worms.Resources.Games
{
    internal class LocalGameRetriever : IResourceRetriever<GameResource>
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

                var startIndex = fileName.IndexOf('[');
                var endIndex = fileName.IndexOf(']');

                var dateString = fileName.Substring(0, startIndex - 1);
                var date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH.mm.ss", null);

                var type = fileName.Substring(startIndex + 1, endIndex - startIndex - 1);
                var teamsString = fileName.Substring(endIndex + 2, fileName.Length - endIndex - 2);
                var teams = teamsString.Split(',').ToList();

                resources.Add(new GameResource(date, "local", type, teams));
            }

            return resources;
        }
    }
}

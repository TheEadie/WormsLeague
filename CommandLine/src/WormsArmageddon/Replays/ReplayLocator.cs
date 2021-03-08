using System.Collections.Generic;
using System.IO.Abstractions;

namespace Worms.WormsArmageddon.Replays
{
    internal class ReplayLocator : IReplayLocator
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;

        public ReplayLocator(IWormsLocator wormsLocator, IFileSystem fileSystem)
        {
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
        }
        public IReadOnlyCollection<string> GetReplayPaths(string pattern)
        {
            var gameInfo = _wormsLocator.Find();

            if (!gameInfo.IsInstalled)
            {
                return new List<string>(0);
            }

            return _fileSystem.Directory.GetFiles(gameInfo.GamesFolder, $"{pattern}*.WAgame");
        }
    }
}

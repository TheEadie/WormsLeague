using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Worms.Armageddon.Game;

namespace Worms.Cli.Resources.Replays
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
        public IReadOnlyCollection<ReplayPaths> GetReplayPaths(string pattern)
        {
            var gameInfo = _wormsLocator.Find();

            if (!gameInfo.IsInstalled)
            {
                return new List<ReplayPaths>(0);
            }

            var waGamePaths = _fileSystem.Directory.GetFiles(gameInfo.ReplayFolder, $"{pattern}*.WAgame");
            return waGamePaths.Select(x => new ReplayPaths(x, GetLogPath(x))).ToList();
        }

        private string GetLogPath(string waGamePath)
        {
            var fileName = _fileSystem.Path.GetFileNameWithoutExtension(waGamePath);
            var folder = _fileSystem.Path.GetDirectoryName(waGamePath);
            var logPath = _fileSystem.Path.Combine(folder, fileName + ".log");
            return _fileSystem.File.Exists(logPath) ? logPath : null;
        }
    }
}

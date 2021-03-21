using System.IO.Abstractions;
using System.Linq;
using Worms.Armageddon.Game.Replays;
using Worms.Armageddon.Resources.Replays;

namespace Worms.Resources.Replays
{
    internal class LocalReplayDeleter : IResourceDeleter<ReplayResource>
    {
        private readonly IReplayLocator _replayLocator;
        private readonly IFileSystem _fileSystem;

        public LocalReplayDeleter(IReplayLocator replayLocator, IFileSystem fileSystem)
        {
            _replayLocator = replayLocator;
            _fileSystem = fileSystem;
        }

        public void Delete(ReplayResource resource)
        {
            var paths = _replayLocator.GetReplayPaths(resource.Date.ToString("yyyy-MM-dd HH.mm.ss")).Single();
            _fileSystem.File.Delete(paths.WAgamePath);
            if (!string.IsNullOrEmpty(paths.LogPath))
            {
                _fileSystem.File.Delete(paths.LogPath);
            }
        }
    }
}

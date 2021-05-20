using System.IO.Abstractions;

namespace Worms.Cli.Resources.Local.Replays
{
    internal class LocalReplayDeleter : IResourceDeleter<LocalReplay>
    {
        private readonly IFileSystem _fileSystem;

        public LocalReplayDeleter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Delete(LocalReplay resource)
        {
            _fileSystem.File.Delete(resource.Paths.WAgamePath);
            if (!string.IsNullOrEmpty(resource.Paths.LogPath))
            {
                _fileSystem.File.Delete(resource.Paths.LogPath);
            }
        }
    }
}

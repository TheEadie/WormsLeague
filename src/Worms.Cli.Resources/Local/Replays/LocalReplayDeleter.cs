using System.IO.Abstractions;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayDeleter(IFileSystem fileSystem) : IResourceDeleter<LocalReplay>
{
    public void Delete(LocalReplay resource)
    {
        fileSystem.File.Delete(resource.Paths.WAgamePath);
        if (!string.IsNullOrEmpty(resource.Paths.LogPath))
        {
            fileSystem.File.Delete(resource.Paths.LogPath);
        }
    }
}

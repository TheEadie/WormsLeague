using System.IO.Abstractions;
using Worms.Armageddon.Game;

namespace Worms.Cli.Resources.Local.Schemes;

internal sealed class LocalSchemeDeleter
    (IWormsLocator wormsLocator, IFileSystem fileSystem) : IResourceDeleter<LocalScheme>
{
    public void Delete(LocalScheme resource)
    {
        var gameInfo = wormsLocator.Find();
        var path = fileSystem.Path.Combine(gameInfo.SchemesFolder, resource.Name + ".wsc");
        fileSystem.File.Delete(path);
    }
}

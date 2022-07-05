using System.IO.Abstractions;
using Worms.Armageddon.Game;

namespace Worms.Cli.Resources.Local.Schemes
{
    internal class LocalSchemeDeleter : IResourceDeleter<LocalScheme>
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;

        public LocalSchemeDeleter(IWormsLocator wormsLocator, IFileSystem fileSystem)
        {
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
        }

        public void Delete(LocalScheme resource)
        {
            var gameInfo = _wormsLocator.Find();
            var path = _fileSystem.Path.Combine(gameInfo.SchemesFolder, resource.Name + ".wsc");
            _fileSystem.File.Delete(path);
        }
    }
}

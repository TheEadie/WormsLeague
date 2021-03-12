using System.IO.Abstractions;
using Worms.Armageddon.Game;

namespace Worms.Resources.Schemes
{
    internal class LocalSchemeDeleter : IResourceDeleter<SchemeResource>
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;

        public LocalSchemeDeleter(IWormsLocator wormsLocator, IFileSystem fileSystem)
        {
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
        }

        public void Delete(SchemeResource resource)
        {
            var gameInfo = _wormsLocator.Find();
            var path = _fileSystem.Path.Combine(gameInfo.SchemesFolder, resource.Name + ".wsc");
            _fileSystem.File.Delete(path);
        }
    }
}

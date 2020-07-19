using System.Collections.Generic;
using System.IO.Abstractions;
using Syroot.Worms.Armageddon;
using Worms.WormsArmageddon;

namespace Worms.Resources.Schemes
{
    internal class LocalSchemesRetriever : ISchemesRetriever
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;

        public LocalSchemesRetriever(IWormsLocator wormsLocator, IFileSystem fileSystem)
        {
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
        }

        public IReadOnlyCollection<SchemeResource> Get(string pattern = "*")
        {
            var gameInfo = _wormsLocator.Find();

            if (!gameInfo.IsInstalled)
            {
                return new List<SchemeResource>(0);
            }

            var schemes = new List<SchemeResource>();

            foreach (var scheme in _fileSystem.Directory.GetFiles(gameInfo.SchemesFolder, $"{pattern}.wsc"))
            {
                var fileName = _fileSystem.Path.GetFileNameWithoutExtension(scheme);
                var details = new Scheme(scheme);
                schemes.Add(new SchemeResource(fileName, "local", details));
            }

            return schemes;
        }
    }
}

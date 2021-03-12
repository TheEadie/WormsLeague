using System.Collections.Generic;
using System.IO.Abstractions;
using Worms.Armageddon.Game;
using Worms.Armageddon.Resources.Schemes;
using Worms.Armageddon.Resources.Schemes.Binary;

namespace Worms.Resources.Schemes
{
    internal class LocalSchemesRetriever : IResourceRetriever<SchemeResource>
    {
        private readonly IWscReader _wscReader;
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;

        public LocalSchemesRetriever(IWscReader wscReader, IWormsLocator wormsLocator, IFileSystem fileSystem)
        {
            _wscReader = wscReader;
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
                var details = _wscReader.Read(scheme);
                schemes.Add(new SchemeResource(fileName, "local", details));
            }

            return schemes;
        }
    }
}

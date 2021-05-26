using System.Collections.Generic;
using System.IO.Abstractions;
using Worms.Armageddon.Game;
using Worms.Armageddon.Resources.Schemes.Binary;

namespace Worms.Cli.Resources.Local.Schemes
{
    internal class LocalSchemesRetriever : IResourceRetriever<LocalScheme>
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

        public IReadOnlyCollection<LocalScheme> Get(string pattern = "*")
        {
            var gameInfo = _wormsLocator.Find();

            if (!gameInfo.IsInstalled)
            {
                return new List<LocalScheme>(0);
            }

            var schemes = new List<LocalScheme>();

            foreach (var scheme in _fileSystem.Directory.GetFiles(gameInfo.SchemesFolder, $"{pattern}.wsc"))
            {
                var fileName = _fileSystem.Path.GetFileNameWithoutExtension(scheme);
                var details = _wscReader.Read(scheme);
                schemes.Add(new LocalScheme(scheme, fileName, details));
            }

            return schemes;
        }
    }
}

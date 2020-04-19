using System.Collections.Generic;
using System.IO.Abstractions;
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

        public IReadOnlyCollection<SchemeResource> Get()
        {
            var schemeFolder = _wormsLocator.Find().SchemesFolder;

            var schemes = new List<SchemeResource>();

            foreach(var scheme in _fileSystem.Directory.GetFiles(schemeFolder, "*.wsc"))
            {
                var fileName = _fileSystem.Path.GetFileNameWithoutExtension(scheme);
                schemes.Add(new SchemeResource(fileName, "local"));
            }

            return schemes;
        }
    }
}
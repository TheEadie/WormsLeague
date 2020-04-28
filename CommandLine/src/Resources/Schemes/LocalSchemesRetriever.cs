using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Worms.WormsArmageddon;
using Worms.WormsArmageddon.Schemes.WscFiles;

namespace Worms.Resources.Schemes
{
    internal class LocalSchemesRetriever : ISchemesRetriever
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly IFileSystem _fileSystem;
        private readonly WscReader _wscReader;

        public LocalSchemesRetriever(IWormsLocator wormsLocator, IFileSystem fileSystem, WscReader wscReader)
        {
            _wormsLocator = wormsLocator;
            _fileSystem = fileSystem;
            _wscReader = wscReader;
        }

        public IReadOnlyCollection<SchemeResource> Get()
        {
            var schemeFolder = _wormsLocator.Find().SchemesFolder;

            var schemes = new List<SchemeResource>();

            foreach(var scheme in _fileSystem.Directory.GetFiles(schemeFolder, "*.wsc"))
            {
                var fileName = _fileSystem.Path.GetFileNameWithoutExtension(scheme);
                var details = _wscReader.GetModel(scheme);
                schemes.Add(new SchemeResource(fileName, "local", details));
            }

            return schemes;
        }

        public SchemeResource Get(string name)
        {
            var schemeFolder = _wormsLocator.Find().SchemesFolder;

            var scheme = _fileSystem.Path.Combine(schemeFolder, $"{name}.wsc");

            if (!_fileSystem.File.Exists(scheme))
            {
                throw new ArgumentException($"Can not find scheme: '{name}'");
            }

            var fileName = _fileSystem.Path.GetFileNameWithoutExtension(scheme);
            var details = _wscReader.GetModel(scheme);
            return new SchemeResource(fileName, "local", details);
        }
    }
}
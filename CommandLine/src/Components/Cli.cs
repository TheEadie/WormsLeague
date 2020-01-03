using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Worms.Components.Repositories;
using Worms.Components.Updates;

namespace Worms.Components
{
    public class Cli : IComponent
    {
        private readonly IUpdateRepository _updateRepository;
        private readonly IComponentUpdater _componentUpdater;
        private readonly IFileSystem _fileSystem;

        public string Name { get; }
        
        public Version InstalledVersion { get; }

        public string ComponentPath { get; }

        public Cli(IUpdateRepository repository, IComponentUpdater componentUpdater, IFileSystem fileSystem)
        {
            _updateRepository = repository;
            _componentUpdater = componentUpdater;
            _fileSystem = fileSystem;

            Name = "CLI";
            InstalledVersion = Assembly.GetEntryAssembly().GetName().Version;
            ComponentPath = _fileSystem.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public IReadOnlyCollection<Version> GetAvailiableVersions()
        {
            return _updateRepository.GetAvailibleVersions(Name).ToList();
        }

        public void Install(Version version)
        {
            var tempPath = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());

            _updateRepository.DownloadVersion(Name, version, tempPath);
            _componentUpdater.Install(tempPath, ComponentPath);

            _fileSystem.Directory.Delete(tempPath, true);
        }
    }
}
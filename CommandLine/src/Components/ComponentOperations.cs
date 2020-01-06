using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Worms.Components.Repositories;
using Worms.Components.Updates;

namespace Worms.Components
{
    public class ComponentOperations
    {
        private readonly IFileSystem _fileSystem;
        private readonly GitHubReleaseRepository _gitHubReleaseRepository;
        private readonly IComponentUpdater _componentUpdater;

        public ComponentOperations(IFileSystem fileSystem, GitHubReleaseRepository gitHubReleaseRepository, IComponentUpdater componentUpdater)
        {
            _fileSystem = fileSystem;
            _gitHubReleaseRepository = gitHubReleaseRepository;
            _componentUpdater = componentUpdater;
        }

        public async Task<IReadOnlyCollection<Version>> GetAvailiableVersions(Component component)
        {
            switch(component.UpdateConfig)
            {
                case OutsideOfToolUpdateConfig config:
                    return config.PossibleVersions;
                case GitHubReleaseUpdateConfig config:
                    _gitHubReleaseRepository.Connect(config);
                    return (await _gitHubReleaseRepository.GetAvailibleVersions(component.Name)).ToList();
                default:
                    throw new ArgumentOutOfRangeException($"Unknown UpdateConfig {typeof(UpdateConfig)}");
            }
        }

        public async Task Install(Component component, Version version)
        {
            switch(component.UpdateConfig)
            {
                case OutsideOfToolUpdateConfig _:
                    return;
                case GitHubReleaseUpdateConfig config:
                    _gitHubReleaseRepository.Connect(config);

                    var tempPath = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());

                    if (_fileSystem.Directory.Exists(tempPath))
                    {
                        _fileSystem.Directory.Delete(tempPath, true);
                    }
                    _fileSystem.Directory.CreateDirectory(tempPath);

                    await _gitHubReleaseRepository.DownloadVersion(component.Name, version, tempPath);
                    _componentUpdater.Install(tempPath, component.ComponentPath);

                    _fileSystem.Directory.Delete(tempPath, true);
                    return;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown UpdateConfig {typeof(UpdateConfig)}");
            }
        }
    }
}
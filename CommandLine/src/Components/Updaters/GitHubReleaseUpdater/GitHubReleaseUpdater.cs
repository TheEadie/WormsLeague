using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Worms.Updates.Installers;
using Worms.Updates.Repositories;

namespace Worms.Components.Updaters
{
    public class GitHubReleaseUpdater : IUpdater<GitHubReleaseUpdateConfig>
    {
        private readonly GitHubReleaseRepository _gitHubReleaseRepository;
        private readonly IFileSystem _fileSystem;
        private readonly IFileCopierInstaller _componentUpdater;

        public GitHubReleaseUpdater(GitHubReleaseRepository gitHubReleaseRepository, IFileSystem fileSystem, IFileCopierInstaller componentUpdater)
        {
            _gitHubReleaseRepository = gitHubReleaseRepository;
            _fileSystem = fileSystem;
            _componentUpdater = componentUpdater;
        }

        public async Task<IReadOnlyCollection<Version>> GetAvailiableVersions(Component component, GitHubReleaseUpdateConfig config)
        {
            _gitHubReleaseRepository.Connect(config.RepoOwner, config.RepoName, config.TagPrefix);
            return (await _gitHubReleaseRepository.GetAvailibleVersions(component.Name)).ToList();
        }

        public async Task Install(Component component, Version version, GitHubReleaseUpdateConfig config)
        {
            _gitHubReleaseRepository.Connect(config.RepoOwner, config.RepoName, config.TagPrefix);

            var tempPath = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());

            if (_fileSystem.Directory.Exists(tempPath))
            {
                _fileSystem.Directory.Delete(tempPath, true);
            }
            _fileSystem.Directory.CreateDirectory(tempPath);

            await _gitHubReleaseRepository.DownloadVersion(component.Name, version, tempPath);
            _componentUpdater.Install(tempPath, component.ComponentPath);

            _fileSystem.Directory.Delete(tempPath, true);
        }
    }
}
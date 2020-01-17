using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Worms.Updates.Installers;
using Worms.Updates.PackageManagers;

namespace Worms.Components.Updaters.GitHubReleaseUpdater
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

        public async Task<IReadOnlyCollection<Version>> GetAvailableVersions(Component component, GitHubReleaseUpdateConfig config)
        {
            _gitHubReleaseRepository.Connect(config.RepoOwner, config.RepoName, config.TagPrefix, config.PersonalAccessToken);
            return (await _gitHubReleaseRepository.GetAvailableVersions(component.Name)).ToList();
        }

        public async Task Install(Component component, Version version, GitHubReleaseUpdateConfig config)
        {
            _gitHubReleaseRepository.Connect(config.RepoOwner, config.RepoName, config.TagPrefix, config.PersonalAccessToken);

            var tempPath = _fileSystem.Path.Combine(component.ComponentPath, ".update");

            if (_fileSystem.Directory.Exists(tempPath))
            {
                _fileSystem.Directory.Delete(tempPath, true);
            }
            _fileSystem.Directory.CreateDirectory(tempPath);

            await _gitHubReleaseRepository.DownloadVersion(component.Name, version, tempPath);
        }
    }
}
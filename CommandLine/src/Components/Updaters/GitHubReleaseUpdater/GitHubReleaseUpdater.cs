using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Worms.Platforms;
using Worms.Updates.PackageManagers;

namespace Worms.Components.Updaters.GitHubReleaseUpdater
{
    public class GitHubReleaseUpdater : IUpdater<GitHubReleaseUpdateConfig>
    {
        private readonly GitHubReleaseRepository _gitHubReleaseRepository;
        private readonly IFileSystem _fileSystem;
        private readonly IPlatformSettings _platformSettings;

        public GitHubReleaseUpdater(GitHubReleaseRepository gitHubReleaseRepository, IFileSystem fileSystem, IPlatformSettings platformSettings)
        {
            _gitHubReleaseRepository = gitHubReleaseRepository;
            _fileSystem = fileSystem;
            _platformSettings = platformSettings;
        }

        public async Task<IReadOnlyCollection<Version>> GetAvailableVersions(Component component, GitHubReleaseUpdateConfig config)
        {
            _gitHubReleaseRepository.Connect(config.RepoOwner, config.RepoName, config.TagPrefix, config.PersonalAccessToken);
            return (await _gitHubReleaseRepository.GetAvailableVersions(component.Name)).ToList();
        }

        public async Task Install(Component component, Version version, GitHubReleaseUpdateConfig config)
        {
            _gitHubReleaseRepository.Connect(config.RepoOwner, config.RepoName, config.TagPrefix, config.PersonalAccessToken);

            var updateFolder = _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Worms", ".update");

            if (_fileSystem.Directory.Exists(updateFolder))
            {
                _fileSystem.Directory.Delete(updateFolder, true);
            }
            _fileSystem.Directory.CreateDirectory(updateFolder);

            var filesToDownload = new Regex($"^{_platformSettings.Identifier}.*$");
            await _gitHubReleaseRepository.DownloadVersion(component.Name, version, updateFolder, filesToDownload);
        }
    }
}
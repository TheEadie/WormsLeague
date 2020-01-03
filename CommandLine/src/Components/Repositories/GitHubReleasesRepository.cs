using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;

namespace Worms.Components.Repositories
{
    public class GitHubReleaseRepository : IUpdateRepository
    {
        private GitHubClient _gitHubClient;
        private string _repoOwner;
        private string _repoName;
        private string _tagPrefix;

        public void Connect(string owner, string repo, string tagPrefix)
        {
            _gitHubClient = new GitHubClient(new ProductHeaderValue("worms-cli"));
            _repoOwner = owner;
            _repoName = repo;
            _tagPrefix = tagPrefix;
        }

        public async Task<IEnumerable<Version>> GetAvailibleVersions(string id)
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(_repoOwner, _repoName);
            var matching = releases.Where(x => x.TagName.StartsWith(_tagPrefix));
            var tagVersions = matching.Select(x => x.TagName.Replace(_tagPrefix, string.Empty));

            var versions = new List<Version>();
            foreach (var tagVersion in tagVersions)
            {
                if (Version.TryParse(tagVersion, out var version))
                {
                    versions.Add(version);
                }
            }
            return versions;
        }

        public async Task DownloadVersion(string id, Version version, string downloadToFolderPath)
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(_repoOwner, _repoName);
            var matching = releases.Single(x => x.TagName == _tagPrefix + version.ToString(3));
            var files = await _gitHubClient.Repository.Release.GetAllAssets(_repoOwner, _repoName, matching.Id);
        }
    }
}
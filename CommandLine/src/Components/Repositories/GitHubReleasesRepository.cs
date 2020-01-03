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
            
            return new List<Version>();

        }

        public void DownloadVersion(string id, Version version, string downloadToFolderPath)
        {
            
        }
    }
}
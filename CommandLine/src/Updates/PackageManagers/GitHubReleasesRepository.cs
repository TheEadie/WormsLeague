using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace Worms.Updates.PackageManagers
{
    public class GitHubReleaseRepository : IUpdateRepository
    {
        private GitHubClient _gitHubClient;
        private string _repoOwner;
        private string _repoName;
        private string _tagPrefix;

        public void Connect(string repoOwner, string repoName, string tagPrefix, string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _gitHubClient = new GitHubClient(new ProductHeaderValue("worms-cli"));
            }
            else
            {
                _gitHubClient = new GitHubClient(new ProductHeaderValue("worms-cli"),
                    new InMemoryCredentialStore(new Credentials(accessToken)));
            }

            _repoOwner = repoOwner;
            _repoName = repoName;
            _tagPrefix = tagPrefix;
        }

        public async Task<IEnumerable<Version>> GetAvailableVersions(string id)
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

        public async Task DownloadVersion(string id, Version version, string downloadToFolderPath, Regex filesToDownload)
        {
            _gitHubClient.SetRequestTimeout(TimeSpan.FromMinutes(10));

            var releases = await _gitHubClient.Repository.Release.GetAll(_repoOwner, _repoName);
            var matching = releases.Single(x => x.TagName == _tagPrefix + version.ToString(3));
            var files = await _gitHubClient.Repository.Release.GetAllAssets(_repoOwner, _repoName, matching.Id);

            foreach(var file in files.Where(x => filesToDownload.IsMatch(x.Name)))
            {
                var raw = await _gitHubClient.Connection.Get<byte[]>(new Uri(file.Url), new Dictionary<string, string>(), "application/octet-stream");
                await File.WriteAllBytesAsync(Path.Combine(downloadToFolderPath, file.Name), raw.Body);
            }
        }
    }
}
using Octokit;
using Octokit.Internal;

namespace Worms.Cli.CommandLine.PackageManagers;

public class GitHubReleasePackageManager
{
    private readonly GitHubClient _gitHubClient;
    private readonly string _repoOwner;
    private readonly string _repoName;
    private readonly string _tagPrefix;

    public GitHubReleasePackageManager(string repoOwner, string repoName, string tagPrefix, string? accessToken)
    {
        _gitHubClient = string.IsNullOrEmpty(accessToken)
            ? new GitHubClient(new ProductHeaderValue("worms-cli"))
            : new GitHubClient(
                new ProductHeaderValue("worms-cli"),
                new InMemoryCredentialStore(new Credentials(accessToken)));

        _gitHubClient.SetRequestTimeout(TimeSpan.FromMinutes(10));

        _repoOwner = repoOwner;
        _repoName = repoName;
        _tagPrefix = tagPrefix;
    }

    public async Task<IEnumerable<Version>> GetAvailableVersions()
    {
        var releases = await _gitHubClient.Repository.Release.GetAll(_repoOwner, _repoName).ConfigureAwait(false);
        var matching = releases.Where(x => x.TagName.StartsWith(_tagPrefix, StringComparison.InvariantCulture));
        var tagVersions = matching.Select(
            x => x.TagName.Replace(_tagPrefix, string.Empty, StringComparison.InvariantCulture));

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

    public async Task DownloadVersion(Version version, string downloadToFolderPath)
    {
        var releases = await _gitHubClient.Repository.Release.GetAll(_repoOwner, _repoName).ConfigureAwait(false);
        var matching = releases.Single(x => x.TagName == _tagPrefix + version.ToString(3));
        var files = await _gitHubClient.Repository.Release.GetAllAssets(_repoOwner, _repoName, matching.Id)
            .ConfigureAwait(false);

        foreach (var file in files)
        {
            var raw = await _gitHubClient.Connection.Get<byte[]>(
                    new Uri(file.Url),
                    new Dictionary<string, string>(),
                    "application/octet-stream")
                .ConfigureAwait(false);
            await File.WriteAllBytesAsync(Path.Combine(downloadToFolderPath, file.Name), raw.Body)
                .ConfigureAwait(false);
        }
    }
}

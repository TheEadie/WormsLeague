namespace Worms.Cli.CommandLine.PackageManagers;

internal sealed class GitHubReleasePackageManagerFactory : IGitHubReleasePackageManagerFactory
{
    public GitHubReleasePackageManager
        Create(string repoOwner, string repoName, string tagPrefix, string? accessToken) =>
        new(repoOwner, repoName, tagPrefix, accessToken);
}

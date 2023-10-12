namespace Worms.Cli.CommandLine.PackageManagers;

internal interface IGitHubReleasePackageManagerFactory
{
    GitHubReleasePackageManager Create(string repoOwner, string repoName, string tagPrefix, string? accessToken);
}

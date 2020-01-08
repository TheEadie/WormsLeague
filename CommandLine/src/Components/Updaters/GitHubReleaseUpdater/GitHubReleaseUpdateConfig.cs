namespace Worms.Components.Updaters.GitHubReleaseUpdater
{
    public class GitHubReleaseUpdateConfig : UpdateConfig
    {
        public string RepoOwner { get; }
        public string RepoName { get; }
        public string TagPrefix { get;} 

        public GitHubReleaseUpdateConfig(string owner, string repo, string tagPrefix)
        {
            RepoOwner = owner;
            RepoName = repo;
            TagPrefix = tagPrefix;
        }
    }
}
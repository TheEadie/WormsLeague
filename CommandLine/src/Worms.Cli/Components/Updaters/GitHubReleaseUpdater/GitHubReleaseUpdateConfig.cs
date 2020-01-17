namespace Worms.Components.Updaters.GitHubReleaseUpdater
{
    public class GitHubReleaseUpdateConfig : UpdateConfig
    {
        public string RepoOwner { get; }
        public string RepoName { get; }
        public string TagPrefix { get;}
        public string PersonalAccessToken { get; }

        public GitHubReleaseUpdateConfig(string owner, string repo, string tagPrefix, string personalAccessToken)
        {
            RepoOwner = owner;
            RepoName = repo;
            TagPrefix = tagPrefix;
            PersonalAccessToken = personalAccessToken;
        }
    }
}
namespace Worms.Configuration
{
    public class Config
    {
        public string GitHubPersonalAccessToken { get; }
        public string SlackAccessToken { get; }

        public Config(string gitHubPersonalAccessToken, string slackAccessToken)
        {
            GitHubPersonalAccessToken = gitHubPersonalAccessToken;
            SlackAccessToken = slackAccessToken;
        }
    }
}
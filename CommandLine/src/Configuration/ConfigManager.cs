using worms.Configuration.SecureStorage;

namespace worms.Configuration
{
    public class ConfigManager
    {
        private readonly ICredentialStorage _credentialStorage;

        public ConfigManager(ICredentialStorage credentialStorage) => _credentialStorage = credentialStorage;

        public Config Load()
        {
            var gitHubPersonalAccessToken = _credentialStorage.Load("Worms.GitHub.AccessToken");
            var slackAccessToken = _credentialStorage.Load("Worms.Slack.AccessToken");

            return new Config(gitHubPersonalAccessToken, slackAccessToken);
        }

        public void Save(Config config)
        {
            _credentialStorage.Store("Worms.GitHub.AccessToken", config.GitHubPersonalAccessToken);
            _credentialStorage.Store("Worms.Slack.AccessToken", config.SlackAccessToken);
        }
    }
}

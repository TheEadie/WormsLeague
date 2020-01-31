using System.IO.Abstractions;
using Newtonsoft.Json;
using Worms.Configuration.SecureStorage;

namespace Worms.Configuration
{
    public class ConfigManager : IConfigManager
    {
        private readonly ICredentialStorage _credentialStorage;
        private readonly IFileSystem _fileSystem;

        public ConfigManager(ICredentialStorage credentialStorage, IFileSystem fileSystem)
        {
            _credentialStorage = credentialStorage;
            _fileSystem = fileSystem;
        }

        public Config Load()
        {
            var localConfig = JsonConvert.DeserializeObject<Config>(_fileSystem.File.ReadAllText("local.json"));
            localConfig.GitHubPersonalAccessToken = _credentialStorage.Load("Worms.GitHub.AccessToken");
            localConfig.SlackAccessToken = _credentialStorage.Load("Worms.Slack.AccessToken");

            return localConfig;
        }

        public void Save(Config config)
        {
            _credentialStorage.Store("Worms.GitHub.AccessToken", config.GitHubPersonalAccessToken);
            _credentialStorage.Store("Worms.Slack.AccessToken", config.SlackAccessToken);

            var localConfig = new Config
            {
                GitHubPersonalAccessToken = "***",
                SlackAccessToken = "***",
                SlackChannel = config.SlackChannel
            };

            _fileSystem.File.WriteAllText("local.json", JsonConvert.SerializeObject(localConfig));
        }
    }
}

using System;
using System.IO.Abstractions;
using System.Text.Json;
using Worms.Configuration.SecureStorage;

namespace Worms.Configuration
{
    internal class ConfigManager : IConfigManager
    {
        private readonly ICredentialStorage _credentialStorage;
        private readonly IFileSystem _fileSystem;
        private readonly string _localConfigPath;

        public ConfigManager(ICredentialStorage credentialStorage, IFileSystem fileSystem)
        {
            _credentialStorage = credentialStorage;
            _fileSystem = fileSystem;

            _localConfigPath = _fileSystem.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs",
                "Worms",
                "local.json");
        }

        public Config Load()
        {
            var localConfig = LoadConfigFromFile(_localConfigPath);

            localConfig.GitHubPersonalAccessToken = _credentialStorage.Load("Worms.GitHub.AccessToken");

            return localConfig;
        }

        private Config LoadConfigFromFile(string configPath)
        {
            if (_fileSystem.File.Exists(configPath))
            {
                var configContent = _fileSystem.File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<Config>(configContent);
            }
            else
            {
                return new Config();
            }
        }

        public void Save(Config config)
        {
            _credentialStorage.Store("Worms.GitHub.AccessToken", config.GitHubPersonalAccessToken);

            var localConfig = new Config
            {
                GitHubPersonalAccessToken = "***",
                SlackWebHook = config.SlackWebHook
            };

            _fileSystem.File.WriteAllText(_localConfigPath, JsonSerializer.Serialize(localConfig));
        }
    }
}

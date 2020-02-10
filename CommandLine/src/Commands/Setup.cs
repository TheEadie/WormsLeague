using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Configuration;

namespace Worms.Commands
{
    [Command("setup", Description = "Interactively set up worms CLI")]
    internal class Setup : CommandBase
    {
        private readonly IConfigManager _configManager;

        [Option(Description = "A GitHub personal access token. Used to increase the number of API calls available", ShortName = "gh")]
        public string GitHubToken { get; }

        [Option(Description = "A Slack web hook. Used to announce games to Slack when hosting", ShortName = "s")]
        public string SlackWebHook { get; }

        public Setup(IConfigManager configManager) => _configManager = configManager;

        public Task<int> OnExecuteAsync()
        {
            var config = _configManager.Load();

            config.GitHubPersonalAccessToken = string.IsNullOrWhiteSpace(GitHubToken) ? Prompt.GetPassword("GitHub Personal Access Token (https://github.com/settings/tokens):") : GitHubToken;
            config.SlackWebHook = string.IsNullOrWhiteSpace(SlackWebHook) ? Prompt.GetString("Slack Web Hook to announce games (Ask the team):") : SlackWebHook;

            _configManager.Save(config);
            return Task.FromResult(0);
        }
    }
}
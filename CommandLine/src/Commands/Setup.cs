using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Configuration;

namespace Worms.Commands
{
    [Command("setup", Description = "Interactively set up worms CLI")]
    internal class Setup : CommandBase
    {
        private readonly IConfigManager _configManager;

        [Option(Description = "A GitHub personal access token. Used to increase the number of API calls available", ShortName = "gt")]
        public string GitHubToken { get; }

        [Option(Description = "A Slack access token. Used to announce games to Slack when hosting", ShortName = "st")]
        public string SlackToken { get; }

        [Option(Description = "A Slack channel name. Used to announce games to Slack when hosting", ShortName = "sc")]
        public string SlackChannel { get; }

        public Setup(IConfigManager configManager) => _configManager = configManager;

        public Task<int> OnExecuteAsync()
        {
            var config = _configManager.Load();

            config.GitHubPersonalAccessToken = string.IsNullOrWhiteSpace(GitHubToken) ? Prompt.GetPassword("GitHub Personal Access Token (https://github.com/settings/tokens):") : GitHubToken;
            config.SlackAccessToken = string.IsNullOrWhiteSpace(SlackToken) ? Prompt.GetPassword("Slack Access Token (https://api.slack.com/custom-integrations/legacy-tokens):") : SlackToken;
            config.SlackChannel = string.IsNullOrWhiteSpace(SlackChannel) ? Prompt.GetString("Slack Channel to announce games:") : SlackChannel;

            _configManager.Save(config);
            return Task.FromResult(0);
        }
    }
}
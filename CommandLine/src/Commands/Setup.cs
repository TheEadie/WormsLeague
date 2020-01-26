using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Configuration;

namespace Worms.Commands
{
    [Command("setup", Description = "Interactively set up worms CLI")]
    internal class Setup : CommandBase
    {
        private readonly ConfigManager _configManager;

        [Option(Description = "A GitHub personal access token. Used to increase the number of API calls available", ShortName = "gt")]
        public string GitHubToken { get; private set; }

        [Option(Description = "A Slack access token. Used to announce games to Slack when hosting", ShortName = "st")]
        public string SlackToken { get; private set; }

        public Setup(ConfigManager configManager) => _configManager = configManager;

        public Task<int> OnExecuteAsync()
        {
            if (string.IsNullOrWhiteSpace(GitHubToken))
            {
                GitHubToken = Prompt.GetPassword("GitHub Personal Access Token (https://github.com/settings/tokens):");
            }

            if (string.IsNullOrWhiteSpace(SlackToken))
            {
                SlackToken =
                    Prompt.GetPassword("Slack Access Token (https://api.slack.com/custom-integrations/legacy-tokens):");
            }

            _configManager.Save(new Config(GitHubToken, SlackToken));
            return Task.FromResult(0);
        }
    }
}
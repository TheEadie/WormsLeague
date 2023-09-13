using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Cli.Configuration;

namespace Worms.Cli.Commands
{
    internal class Setup : Command
    {
        public static readonly Option<string> GitHubToken = new(
            new []{ "--git-hub-token", "-gh"},
            "A GitHub personal access token. Used to increase the number of API calls available");

        public static readonly Option<string> SlackWebHook = new(
            new []{ "--slack-web-hook", "-s"},
            "A Slack web hook. Used to announce games to Slack when hosting");

        public Setup() : base("setup", "Interactively set up Worms CLI")
        {
            AddOption(GitHubToken);
            AddOption(SlackWebHook);
        }
    }

    internal class SetupHandler : ICommandHandler
    {
        private readonly IConfigManager _configManager;
        private readonly ILogger _logger;

        public SetupHandler(IConfigManager configManager, ILogger logger)
        {
            _configManager = configManager;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public Task<int> InvokeAsync(InvocationContext context)
        {
            var githubToken = context.ParseResult.GetValueForOption(Setup.GitHubToken);
            var slackWebHook = context.ParseResult.GetValueForOption(Setup.SlackWebHook);

            var config = _configManager.Load();

            if (string.IsNullOrWhiteSpace(githubToken))
            {
                _logger.Information(
                    "GitHub Personal Access Token (Scopes: 'public_repo' only) (https://github.com/settings/tokens):");
                config.GitHubPersonalAccessToken = Console.ReadLine();
            }
            else
            {
                config.GitHubPersonalAccessToken = githubToken;
            }

            if (string.IsNullOrWhiteSpace(slackWebHook))
            {
                _logger.Information("Slack Web Hook to announce games (Ask the team):");
                config.SlackWebHook = Console.ReadLine();
            }
            else
            {
                config.SlackWebHook = slackWebHook;
            }

            _configManager.Save(config);
            return Task.FromResult(0);
        }
    }
}

using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Cli.Configuration;

namespace Worms.Cli.Commands;

internal sealed class Setup : Command
{
    public static readonly Option<string> GitHubToken = new(
        new[]
        {
            "--git-hub-token",
            "-gh"
        },
        "A GitHub personal access token. Used to increase the number of API calls available");

    public static readonly Option<string> SlackWebHook = new(
        new[]
        {
            "--slack-web-hook",
            "-s"
        },
        "A Slack web hook. Used to announce games to Slack when hosting");

    public Setup()
        : base("setup", "Interactively set up Worms CLI")
    {
        AddOption(GitHubToken);
        AddOption(SlackWebHook);
    }
}

internal sealed class SetupHandler(IConfigManager configManager, ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var githubToken = context.ParseResult.GetValueForOption(Setup.GitHubToken);
        var slackWebHook = context.ParseResult.GetValueForOption(Setup.SlackWebHook);

        var config = configManager.Load();

        if (string.IsNullOrWhiteSpace(githubToken))
        {
            logger.Information(
                "GitHub Personal Access Token (Scopes: 'public_repo' only) (https://github.com/settings/tokens):");
            config.GitHubPersonalAccessToken = Console.ReadLine()!;
        }
        else
        {
            config.GitHubPersonalAccessToken = githubToken;
        }

        if (string.IsNullOrWhiteSpace(slackWebHook))
        {
            logger.Information("Slack Web Hook to announce games (Ask the team):");
            config.SlackWebHook = Console.ReadLine()!;
        }
        else
        {
            config.SlackWebHook = slackWebHook;
        }

        configManager.Save(config);
        return Task.FromResult(0);
    }
}

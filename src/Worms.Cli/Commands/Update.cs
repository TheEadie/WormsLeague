using System.CommandLine;
using System.CommandLine.Invocation;
using Octokit;
using Serilog;
using Worms.Cli.CommandLine;
using Worms.Cli.Configuration;

namespace Worms.Cli.Commands;

internal sealed class Update : Command
{
    public Update()
        : base("update", "Update Worms CLI") { }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class UpdateHandler
    (IConfigManager configManager, CliUpdater cliUpdater, ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) => Task.Run(async () => await InvokeAsync(context)).Result;

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        try
        {
            var config = configManager.Load();
            await cliUpdater.DownloadLatestUpdate(config, logger).ConfigureAwait(false);
        }
        catch (RateLimitExceededException)
        {
            logger.Error(
                "Could not check for updates: GitHub API rate limit has been exceeded. Please run 'worms setup' and provide a personal access token.");
            return 1;
        }

        return 0;
    }
}

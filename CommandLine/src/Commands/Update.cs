using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Octokit;
using Worms.Cli;
using Worms.Configuration;

namespace Worms.Commands
{
    [Command("update", Description = "Update worms CLI")]
    internal class Update : CommandBase
    {
        private readonly IConfigManager _configManager;
        private readonly CliUpdater _cliUpdater;

        public Update(IConfigManager configManager, CliUpdater cliUpdater)
        {
            _configManager = configManager;
            _cliUpdater = cliUpdater;
        }

        public async Task<int> OnExecuteAsync()
        {
            try
            {
                await UpdateComponent().ConfigureAwait(false);
            }
            catch (RateLimitExceededException)
            {
                Logger.Error(
                    "Could not check for updates: GitHub API rate limit has been exceeded. Please run 'worms setup' and provide a personal access token.");
                return 1;
            }

            return 0;
        }

        private async Task UpdateComponent()
        {
            var config = _configManager.Load();
            await _cliUpdater.DownloadLatestUpdate(config, Logger).ConfigureAwait(false);
        }
    }
}

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Octokit;
using Serilog;
using Worms.Cli;
using Worms.Configuration;

namespace Worms.Commands
{
    internal class Update : Command
    {
        public Update() : base("update", "Update Worms CLI") { }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class UpdateHandler : ICommandHandler
    {
        private readonly IConfigManager _configManager;
        private readonly CliUpdater _cliUpdater;
        private readonly ILogger _logger;

        public UpdateHandler(IConfigManager configManager, CliUpdater cliUpdater, ILogger logger)
        {
            _configManager = configManager;
            _cliUpdater = cliUpdater;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) => 
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                var config = _configManager.Load();
                await _cliUpdater.DownloadLatestUpdate(config, _logger).ConfigureAwait(false);
            }
            catch (RateLimitExceededException)
            {
                _logger.Error(
                    "Could not check for updates: GitHub API rate limit has been exceeded. Please run 'worms setup' and provide a personal access token.");
                return 1;
            }

            return 0;
        }
    }
}

using System.Net;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Configuration;
using Worms.League;
using Worms.Slack;
using Worms.WormsArmageddon;

namespace Worms.Commands
{
    [Command("host", Description = "Host a game of worms using the latest options")]
    internal class Host : CommandBase
    {
        private readonly IWormsRunner _wormsRunner;
        private readonly ISlackAnnouncer _slackAnnouncer;
        private readonly IConfigManager _configManager;
        private readonly IWormsLocator _wormsLocator;
        private readonly LeagueUpdater _leagueUpdater;

        public Host(IWormsLocator wormsLocator, IWormsRunner wormsRunner, ISlackAnnouncer slackAnnouncer, IConfigManager configManager, LeagueUpdater leagueUpdater)
        {
            _wormsRunner = wormsRunner;
            _slackAnnouncer = slackAnnouncer;
            _configManager = configManager;
            _wormsLocator = wormsLocator;
            _leagueUpdater = leagueUpdater;
        }

        public async Task<int> OnExecuteAsync()
        {
            Logger.Verbose("Loading configuration");
            var config = _configManager.Load();
            var hostName = Dns.GetHostName();
            var gameInfo = _wormsLocator.Find();

            if (!gameInfo.IsInstalled)
            {
                Logger.Error("Worms Armageddon is not installed");
                return 1;
            }

            Logger.Information("Downloading the latest options");
            await _leagueUpdater.Update(config, Logger).ConfigureAwait(false);

            Logger.Information("Starting Worms Armageddon");
            var runGame = _wormsRunner.RunWorms("wa://").ConfigureAwait(false);

            Logger.Information($"Announcing game on Slack {config.SlackChannel}");
            Logger.Verbose($"Host name: {hostName}");
            await _slackAnnouncer.AnnounceGameStarting(hostName, config.SlackAccessToken, config.SlackChannel, Logger).ConfigureAwait(false);

            await runGame;
            return 0;
        }
    }
}
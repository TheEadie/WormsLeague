using System.Net;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Configuration;
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

        public Host(IWormsRunner wormsRunner, ISlackAnnouncer slackAnnouncer, IConfigManager configManager)
        {
            _wormsRunner = wormsRunner;
            _slackAnnouncer = slackAnnouncer;
            _configManager = configManager;
        }

        public async Task<int> OnExecuteAsync()
        {
            var runGame = _wormsRunner.RunWorms("wa://").ConfigureAwait(false);

            var config = _configManager.Load();
            var hostName = Dns.GetHostName();
            await _slackAnnouncer.AnnounceGameStarting(hostName, config.SlackAccessToken, config.SlackChannel, Logger).ConfigureAwait(false);

            await runGame;
            return 0;
        }
    }
}
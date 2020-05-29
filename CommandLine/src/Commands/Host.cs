using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Configuration;
using Worms.League;
using Worms.Slack;
using Worms.WormsArmageddon;

// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

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

        public Host(
            IWormsLocator wormsLocator,
            IWormsRunner wormsRunner,
            ISlackAnnouncer slackAnnouncer,
            IConfigManager configManager,
            LeagueUpdater leagueUpdater)
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

            string hostIp;
            try
            {
                const string domain = "red-gate.com";
                hostIp = GetIpAddress(domain);
            }
            catch (Exception e)
            {
                Logger.Error($"IP address could not be found. {e.Message}");
                return 1;
            }

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

            Logger.Information("Announcing game on Slack");
            Logger.Verbose($"Host name: {hostIp}");
            await _slackAnnouncer.AnnounceGameStarting(hostIp, config.SlackWebHook, Logger).ConfigureAwait(false);

            await runGame;
            return 0;
        }

        private static string GetIpAddress(string domain)
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            var leagueNetworkAdapter = adapters.SingleOrDefault(x => x.GetIPProperties().DnsSuffix == domain);

            if (leagueNetworkAdapter is null)
            {
                throw new Exception("No network adapter for domain: {domain}");
            }

            var hostIp = leagueNetworkAdapter.GetIPProperties().UnicastAddresses.FirstOrDefault()?.Address.ToString();

            if (hostIp is null)
            {
                throw new Exception("No IP address found for network adapter: {leagueNetworkAdapter.Name}");
            }

            return hostIp;
        }
    }
}

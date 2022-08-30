using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Armageddon.Game;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Remote.Games;
using Worms.Configuration;
using Worms.League;
using Worms.Slack;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
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
        private readonly IResourceCreator<RemoteGame, string> _remoteGameCreator;
        private readonly IRemoteGameUpdater _gameUpdater;

        [Option(Description = "When set the CLI will print what will happen rather than running the commands",
            LongName = "dry-run", ShortName = "dr")]
        public bool DryRun { get; }

        public Host(
            IWormsLocator wormsLocator,
            IWormsRunner wormsRunner,
            ISlackAnnouncer slackAnnouncer,
            IConfigManager configManager,
            LeagueUpdater leagueUpdater,
            IResourceCreator<RemoteGame, string> remoteGameCreator,
            IRemoteGameUpdater gameUpdater)
        {
            _wormsRunner = wormsRunner;
            _slackAnnouncer = slackAnnouncer;
            _configManager = configManager;
            _wormsLocator = wormsLocator;
            _leagueUpdater = leagueUpdater;
            _remoteGameCreator = remoteGameCreator;
            _gameUpdater = gameUpdater;
        }

        public async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
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
            if (!DryRun)
            {
                await _leagueUpdater.Update(config, Logger).ConfigureAwait(false);
            }

            Logger.Information("Starting Worms Armageddon");
            var runGame = !DryRun ? _wormsRunner.RunWorms("wa://") : Task.CompletedTask;

            Logger.Information("Announcing game on Slack");
            Logger.Verbose($"Host name: {hostIp}");
            if (!DryRun)
            {
                await _slackAnnouncer.AnnounceGameStarting(hostIp, config.SlackWebHook, Logger).ConfigureAwait(false);
            }

            Logger.Information("Announcing game to hub");
            RemoteGame game = null;
            if (!DryRun)
            {
                game = await _remoteGameCreator.Create(hostIp, Logger, cancellationToken);
            }

            await runGame;

            Logger.Information("Marking game as complete in hub");
            if (!DryRun)
            {
                await _gameUpdater.SetGameComplete(game, Logger, cancellationToken);
            }

            return 0;
        }

        private static string GetIpAddress(string domain)
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            var leagueNetworkAdapter = adapters.FirstOrDefault(x =>
                x.GetIPProperties().DnsSuffix == domain &&
                x.OperationalStatus == OperationalStatus.Up);

            if (leagueNetworkAdapter is null)
            {
                throw new Exception($"No network adapter for domain: {domain}");
            }

            var hostIp = leagueNetworkAdapter.GetIPProperties()
                .UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                ?.Address.ToString();

            if (hostIp is null)
            {
                throw new Exception($"No IPv4 address found for network adapter: {leagueNetworkAdapter.Name}");
            }

            return hostIp;
        }
    }
}
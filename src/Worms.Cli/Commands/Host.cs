using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Remote.Replays;
using Worms.Configuration;
using Worms.League;
using Worms.Slack;

namespace Worms.Commands
{
    internal class Host : Command
    {
        public static readonly Option<bool> DryRun = new(
            new[]
            {
                "--dry-run",
                "-dr"
            },
            "When set the CLI will print what will happen rather than running the commands");

        public static readonly Option<bool> LocalMode = new(
            new[] { "--local-mode" },
            "Use legacy local mode to announce to Slack");

        public static readonly Option<bool> SkipSchemeDownload = new(
            new[] { "--skip-scheme-download" },
            "Don't download the latest schemes before starting the game");

        public static readonly Option<bool> SkipUpload = new(
            new[] { "--skip-upload" },
            "Don't Upload the replay to Worms Hub when the game finishes");

        public static readonly Option<bool> SkipAnnouncement = new(
            new[] { "--skip-announcement" },
            "Don't announce the game to Slack or Worms Hub");

        public Host()
            : base("host", "Host a game of worms using the latest options")
        {
            AddOption(DryRun);
            AddOption(LocalMode);
            AddOption(SkipSchemeDownload);
            AddOption(SkipUpload);
            AddOption(SkipAnnouncement);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class HostHandler : ICommandHandler
    {
        private readonly IWormsRunner _wormsRunner;
        private readonly ISlackAnnouncer _slackAnnouncer;
        private readonly IConfigManager _configManager;
        private readonly IWormsLocator _wormsLocator;
        private readonly LeagueUpdater _leagueUpdater;
        private readonly IResourceCreator<RemoteGame, string> _remoteGameCreator;
        private readonly IRemoteGameUpdater _gameUpdater;
        private readonly IResourceRetriever<LocalReplay> _localReplayRetriever;
        private readonly IResourceCreator<RemoteReplay, RemoteReplayCreateParameters> _remoteReplayCreator;
        private readonly ILogger _logger;

        public HostHandler(
            IWormsLocator wormsLocator,
            IWormsRunner wormsRunner,
            ISlackAnnouncer slackAnnouncer,
            IConfigManager configManager,
            LeagueUpdater leagueUpdater,
            IResourceCreator<RemoteGame, string> remoteGameCreator,
            IRemoteGameUpdater gameUpdater,
            IResourceRetriever<LocalReplay> localReplayRetriever,
            IResourceCreator<RemoteReplay, RemoteReplayCreateParameters> remoteReplayCreator,
            ILogger logger)
        {
            _wormsRunner = wormsRunner;
            _slackAnnouncer = slackAnnouncer;
            _configManager = configManager;
            _wormsLocator = wormsLocator;
            _leagueUpdater = leagueUpdater;
            _remoteGameCreator = remoteGameCreator;
            _gameUpdater = gameUpdater;
            _localReplayRetriever = localReplayRetriever;
            _remoteReplayCreator = remoteReplayCreator;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) => Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var cancellationToken = context.GetCancellationToken();
            var dryRun = context.ParseResult.GetValueForOption(Host.DryRun);
            var localMode = context.ParseResult.GetValueForOption(Host.LocalMode);
            var skipUpload = context.ParseResult.GetValueForOption(Host.SkipUpload);
            var skipAnnouncement = context.ParseResult.GetValueForOption(Host.SkipAnnouncement);
            var skipSchemeDownload = context.ParseResult.GetValueForOption(Host.SkipSchemeDownload);

            _logger.Verbose("Loading configuration");
            var config = _configManager.Load();

            string hostIp;
            try
            {
                const string domain = "red-gate.com";
                hostIp = GetIpAddress(domain);
            }
            catch (Exception e)
            {
                _logger.Error($"IP address could not be found. {e.Message}");
                return 1;
            }

            var gameInfo = _wormsLocator.Find();

            if (!gameInfo.IsInstalled)
            {
                _logger.Error("Worms Armageddon is not installed");
                return 1;
            }

            await DownloadLatestOptions(skipSchemeDownload, config, dryRun);
            var runGame = StartWorms(dryRun, cancellationToken);

            if (localMode)
            {
                await AnnounceGameLocal(hostIp, skipAnnouncement, config, dryRun);
                await WaitForGameToClose(runGame);
                return 0;
            }

            var game = await AnnounceGameToWormsHub(hostIp, skipAnnouncement, dryRun, cancellationToken);
            await WaitForGameToClose(runGame);
            await MarkGameCompleteOnWormsHub(game, skipAnnouncement, dryRun, cancellationToken);
            await UploadReplayToWormsHub(skipUpload, dryRun, cancellationToken);
            return 0;
        }

        private async Task<RemoteGame> AnnounceGameToWormsHub(
            string hostIp,
            bool skipAnnouncement,
            bool dryRun,
            CancellationToken cancellationToken)
        {
            if (skipAnnouncement)
            {
                return new RemoteGame("", "", "");
            }

            _logger.Information("Announcing game to hub");
            RemoteGame game = null;
            if (!dryRun)
            {
                game = await _remoteGameCreator.Create(hostIp, _logger, cancellationToken);
            }

            return game;
        }

        private async Task MarkGameCompleteOnWormsHub(
            RemoteGame game,
            bool skipAnnouncement,
            bool dryRun,
            CancellationToken cancellationToken)
        {
            if (skipAnnouncement)
            {
                return;
            }

            _logger.Information("Marking game as complete in hub");
            if (!dryRun)
            {
                await _gameUpdater.SetGameComplete(game, _logger, cancellationToken);
            }
        }

        private async Task UploadReplayToWormsHub(bool skipUpload, bool dryRun, CancellationToken cancellationToken)
        {
            if (skipUpload)
            {
                return;
            }

            _logger.Information("Uploading replay to hub");
            var allReplays = await _localReplayRetriever.Get(_logger, cancellationToken);
            var replay = allReplays.MaxBy(x => x.Details.Date);
            _logger.Information("Uploading replay: {ReplayPath}", replay.Paths.WAgamePath);
            if (!dryRun)
            {
                await _remoteReplayCreator.Create(
                    new RemoteReplayCreateParameters(replay.Details.Date.ToString("s"), replay.Paths.WAgamePath),
                    _logger,
                    cancellationToken);
            }
        }

        private async Task AnnounceGameLocal(string hostIp, bool skipAnnouncement, Config config, bool dryRun)
        {
            if (skipAnnouncement)
            {
                return;
            }

            _logger.Information("Announcing game on Slack");
            _logger.Verbose($"Host name: {hostIp}");
            if (!dryRun)
            {
                await _slackAnnouncer.AnnounceGameStarting(hostIp, config.SlackWebHook, _logger).ConfigureAwait(false);
            }
        }

        private static string GetIpAddress(string domain)
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            var leagueNetworkAdapter = adapters.FirstOrDefault(
                x => x.GetIPProperties().DnsSuffix == domain && x.OperationalStatus == OperationalStatus.Up);

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

        private async Task DownloadLatestOptions(bool skipSchemeDownload, Config config, bool dryRun)
        {
            if (skipSchemeDownload)
            {
                return;
            }

            _logger.Information("Downloading the latest options");
            if (!dryRun)
            {
                await _leagueUpdater.Update(config, _logger).ConfigureAwait(false);
            }
        }

        private Task StartWorms(bool dryRun, CancellationToken cancellationToken)
        {
            _logger.Information("Starting Worms Armageddon");
            var runGame = !dryRun ? _wormsRunner.RunWorms("wa://") : Task.Delay(5000, cancellationToken);
            return runGame;
        }

        private async Task WaitForGameToClose(Task runGame)
        {
            _logger.Information("Waiting for game to finish");
            await runGame;
        }
    }
}

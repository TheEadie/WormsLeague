using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Configuration;
using Worms.Cli.League;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Remote.Replays;
using Worms.Cli.Slack;

namespace Worms.Cli.Commands;

internal sealed class Host : Command
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

internal sealed class HostHandler(
    IWormsLocator wormsLocator,
    IWormsRunner wormsRunner,
    ISlackAnnouncer slackAnnouncer,
    IConfigManager configManager,
    LeagueUpdater leagueUpdater,
    IResourceCreator<RemoteGame, string> remoteGameCreator,
    IRemoteGameUpdater gameUpdater,
    IResourceRetriever<LocalReplay> localReplayRetriever,
    IResourceCreator<RemoteReplay, RemoteReplayCreateParameters> remoteReplayCreator,
    ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).Result;

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();
        var dryRun = context.ParseResult.GetValueForOption(Host.DryRun);
        var localMode = context.ParseResult.GetValueForOption(Host.LocalMode);
        var skipUpload = context.ParseResult.GetValueForOption(Host.SkipUpload);
        var skipAnnouncement = context.ParseResult.GetValueForOption(Host.SkipAnnouncement);
        var skipSchemeDownload = context.ParseResult.GetValueForOption(Host.SkipSchemeDownload);

        logger.Verbose("Loading configuration");
        var config = configManager.Load();

        string hostIp;
        try
        {
            const string domain = "red-gate.com";
            hostIp = GetIpAddress(domain);
        }
        catch (ConfigurationException e)
        {
            logger.Error($"IP address could not be found. {e.Message}");
            return 1;
        }

        var gameInfo = wormsLocator.Find();

        if (!gameInfo.IsInstalled)
        {
            logger.Error("Worms Armageddon is not installed");
            return 1;
        }

        await DownloadLatestOptions(skipSchemeDownload, config, dryRun).ConfigureAwait(false);
        var runGame = StartWorms(dryRun, cancellationToken);

        if (localMode)
        {
            await AnnounceGameLocal(hostIp, skipAnnouncement, config, dryRun).ConfigureAwait(false);
            await WaitForGameToClose(runGame).ConfigureAwait(false);
            return 0;
        }

        var game = await AnnounceGameToWormsHub(hostIp, skipAnnouncement, dryRun, cancellationToken)
            .ConfigureAwait(false);
        await WaitForGameToClose(runGame).ConfigureAwait(false);
        await MarkGameCompleteOnWormsHub(game, skipAnnouncement, dryRun, cancellationToken).ConfigureAwait(false);
        await UploadReplayToWormsHub(skipUpload, dryRun, cancellationToken).ConfigureAwait(false);
        return 0;
    }

    private async Task<RemoteGame?> AnnounceGameToWormsHub(
        string hostIp,
        bool skipAnnouncement,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (skipAnnouncement)
        {
            return new RemoteGame("", "", "");
        }

        logger.Information("Announcing game to hub");
        RemoteGame? game = null;
        if (!dryRun)
        {
            game = await remoteGameCreator.Create(hostIp, logger, cancellationToken).ConfigureAwait(false);
        }

        return game;
    }

    private async Task MarkGameCompleteOnWormsHub(
        RemoteGame? game,
        bool skipAnnouncement,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (skipAnnouncement)
        {
            return;
        }

        logger.Information("Marking game as complete in hub");
        if (!dryRun)
        {
            await gameUpdater.SetGameComplete(game!, logger, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task UploadReplayToWormsHub(bool skipUpload, bool dryRun, CancellationToken cancellationToken)
    {
        if (skipUpload)
        {
            return;
        }

        logger.Information("Uploading replay to hub");
        var allReplays = await localReplayRetriever.Retrieve(logger, cancellationToken).ConfigureAwait(false);
        var replay = allReplays.MaxBy(x => x.Details.Date);

        if (replay is null)
        {
            logger.Warning("No replay found to upload");
            return;
        }

        logger.Information("Uploading replay: {ReplayPath}", replay.Paths.WAgamePath);
        if (!dryRun)
        {
            _ = await remoteReplayCreator.Create(
                    new RemoteReplayCreateParameters(replay.Details.Date.ToString("s"), replay.Paths.WAgamePath),
                    logger,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task AnnounceGameLocal(string hostIp, bool skipAnnouncement, Config config, bool dryRun)
    {
        if (skipAnnouncement)
        {
            return;
        }

        logger.Information("Announcing game on Slack");
        logger.Verbose($"Host name: {hostIp}");
        if (!dryRun)
        {
            await slackAnnouncer.AnnounceGameStarting(hostIp, config.SlackWebHook, logger).ConfigureAwait(false);
        }
    }

    private static string GetIpAddress(string domain)
    {
        var adapters = NetworkInterface.GetAllNetworkInterfaces();
        var leagueNetworkAdapter = adapters.FirstOrDefault(
                x => x.GetIPProperties().DnsSuffix == domain && x.OperationalStatus == OperationalStatus.Up)
            ?? throw new ConfigurationException($"No network adapter for domain: {domain}");

        var hostIp =
            (leagueNetworkAdapter.GetIPProperties()
                .UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                ?.Address.ToString())
            ?? throw new ConfigurationException(
                $"No IPv4 address found for network adapter: {leagueNetworkAdapter.Name}");

        return hostIp;
    }

    private Task DownloadLatestOptions(bool skipSchemeDownload, Config config, bool dryRun)
    {
        if (skipSchemeDownload)
        {
            return Task.CompletedTask;
        }

        logger.Information("Downloading the latest options");
        return !dryRun ? leagueUpdater.Update(config, logger) : Task.CompletedTask;
    }

    private Task StartWorms(bool dryRun, CancellationToken cancellationToken)
    {
        logger.Information("Starting Worms Armageddon");
        var runGame = !dryRun ? wormsRunner.RunWorms("wa://") : Task.Delay(5000, cancellationToken);
        return runGame;
    }

    private Task WaitForGameToClose(Task runGame)
    {
        logger.Information("Waiting for game to finish");
        return runGame;
    }
}

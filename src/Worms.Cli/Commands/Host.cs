using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.League;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Remote.Replays;

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
        AddOption(SkipSchemeDownload);
        AddOption(SkipUpload);
        AddOption(SkipAnnouncement);
    }
}

internal sealed class HostHandler(
    IWormsLocator wormsLocator,
    IWormsRunner wormsRunner,
    LeagueUpdater leagueUpdater,
    IResourceCreator<RemoteGame, string> remoteGameCreator,
    IRemoteGameUpdater gameUpdater,
    IResourceRetriever<LocalReplay> localReplayRetriever,
    IResourceCreator<RemoteReplay, RemoteReplayCreateParameters> remoteReplayCreator,
    ILogger logger) : ICommandHandler
{
    private const string LeagueName = "redgate";
    private const string Domain = "red-gate.com";

    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();
        var dryRun = context.ParseResult.GetValueForOption(Host.DryRun);
        var skipUpload = context.ParseResult.GetValueForOption(Host.SkipUpload);
        var skipAnnouncement = context.ParseResult.GetValueForOption(Host.SkipAnnouncement);
        var skipSchemeDownload = context.ParseResult.GetValueForOption(Host.SkipSchemeDownload);

        string hostIp;
        try
        {
            hostIp = GetIpAddress(Domain);
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

        await DownloadLatestOptions(skipSchemeDownload, dryRun).ConfigureAwait(false);
        var runGame = StartWorms(dryRun, cancellationToken);

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

    private static string GetIpAddress(string domain)
    {
        var adapters = NetworkInterface.GetAllNetworkInterfaces();
        var leagueNetworkAdapter =
            Array.Find(
                adapters,
                x => x.GetIPProperties().DnsSuffix == domain && x.OperationalStatus == OperationalStatus.Up)
            ?? throw new ConfigurationException($"No network adapter for domain: {domain}");

        return leagueNetworkAdapter.GetIPProperties()
                .UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                ?.Address.ToString()
            ?? throw new ConfigurationException(
                $"No IPv4 address found for network adapter: {leagueNetworkAdapter.Name}");
    }

    private Task DownloadLatestOptions(bool skipSchemeDownload, bool dryRun)
    {
        if (skipSchemeDownload)
        {
            return Task.CompletedTask;
        }

        logger.Information("Downloading the latest options");
        return !dryRun ? leagueUpdater.Update(LeagueName, logger) : Task.CompletedTask;
    }

    private Task StartWorms(bool dryRun, CancellationToken cancellationToken)
    {
        logger.Information("Starting Worms Armageddon");
        return !dryRun ? wormsRunner.RunWorms("wa://") : Task.Delay(5000, cancellationToken);
    }

    private Task WaitForGameToClose(Task runGame)
    {
        logger.Information("Waiting for game to finish");
        return runGame;
    }
}

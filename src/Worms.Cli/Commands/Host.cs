using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
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
    ILogger<HostHandler> logger) : ICommandHandler
{
    private const string LeagueName = "redgate";
    private const string Domain = "red-gate.com";

    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();
        var config = new Config(
            context.ParseResult.GetValueForOption(Host.DryRun),
            context.ParseResult.GetValueForOption(Host.SkipSchemeDownload),
            context.ParseResult.GetValueForOption(Host.SkipUpload),
            context.ParseResult.GetValueForOption(Host.SkipAnnouncement),
            GetIpAddress(Domain),
            wormsLocator.Find()).Validate(
            new RulesFor<Config>().Add(x => x.IpAddress.IsValid, x => x.IpAddress.Error.First())
                .Add(x => x.GameInfo.IsInstalled, "Worms Armageddon is not installed")
                .Build());

        if (!config.IsValid)
        {
            config.LogErrors(logger);
            return 1;
        }

        await HostGame(config.Value, cancellationToken).ConfigureAwait(false);
        return 0;
    }

    private async Task HostGame(Config config, CancellationToken cancellationToken)
    {
        await DownloadLatestOptions(config.SkipSchemeDownload, config.DryRun).ConfigureAwait(false);
        var runGame = StartWorms(config.DryRun, cancellationToken);

        var game = await AnnounceGameToWormsHub(
                config.IpAddress.Value!,
                config.SkipAnnouncement,
                config.DryRun,
                cancellationToken)
            .ConfigureAwait(false);
        await WaitForGameToClose(runGame).ConfigureAwait(false);
        await MarkGameCompleteOnWormsHub(game, config.SkipAnnouncement, config.DryRun, cancellationToken)
            .ConfigureAwait(false);
        await UploadReplayToWormsHub(config.SkipUpload, config.DryRun, cancellationToken).ConfigureAwait(false);
    }

    private sealed record Config(
        bool DryRun,
        bool SkipSchemeDownload,
        bool SkipUpload,
        bool SkipAnnouncement,
        Validated<string> IpAddress,
        GameInfo GameInfo);

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

        logger.LogInformation("Announcing game to hub");
        RemoteGame? game = null;
        if (!dryRun)
        {
            game = await remoteGameCreator.Create(hostIp, cancellationToken).ConfigureAwait(false);
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

        logger.LogInformation("Marking game as complete in hub");
        if (!dryRun)
        {
            await gameUpdater.SetGameComplete(game!, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task UploadReplayToWormsHub(bool skipUpload, bool dryRun, CancellationToken cancellationToken)
    {
        if (skipUpload)
        {
            return;
        }

        logger.LogInformation("Uploading replay to hub");
        var allReplays = await localReplayRetriever.Retrieve(cancellationToken).ConfigureAwait(false);
        var replay = allReplays.MaxBy(x => x.Details.Date);

        if (replay is null)
        {
            logger.LogWarning("No replay found to upload");
            return;
        }

        logger.LogInformation("Uploading replay: {ReplayPath}", replay.Paths.WAgamePath);
        if (!dryRun)
        {
            _ = await remoteReplayCreator.Create(
                    new RemoteReplayCreateParameters(replay.Details.Date.ToString("s"), replay.Paths.WAgamePath),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static Validated<string> GetIpAddress(string domain)
    {
        var adapters = NetworkInterface.GetAllNetworkInterfaces();
        var leagueNetworkAdapter = Array.Find(
            adapters,
            x => x.GetIPProperties().DnsSuffix == domain && x.OperationalStatus == OperationalStatus.Up);

        if (leagueNetworkAdapter is null)
        {
            return new Invalid<string>($"No network adapter found for domain: {domain}");
        }

        var ipAddress = leagueNetworkAdapter.GetIPProperties()
            .UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
            ?.Address.ToString();

        return ipAddress is null
            ? new Invalid<string>($"No IPv4 address found for domain: {domain}")
            : new Valid<string>(ipAddress);
    }

    private Task DownloadLatestOptions(bool skipSchemeDownload, bool dryRun)
    {
        if (skipSchemeDownload)
        {
            return Task.CompletedTask;
        }

        logger.LogInformation("Downloading the latest options");
        return !dryRun ? leagueUpdater.Update(LeagueName) : Task.CompletedTask;
    }

    private Task StartWorms(bool dryRun, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Worms Armageddon");
        return !dryRun ? wormsRunner.RunWorms("wa://") : Task.Delay(5000, cancellationToken);
    }

    private Task WaitForGameToClose(Task runGame)
    {
        logger.LogInformation("Waiting for game to finish");
        return runGame;
    }
}

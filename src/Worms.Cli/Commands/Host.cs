using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Commands.Validation;
using Worms.Cli.League;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Remote.Replays;

namespace Worms.Cli.Commands;

internal sealed class Host : Command
{
    public static readonly Option<bool> DryRun = new("--dry-run", "-dr")
    {
        Description = "When set the CLI will print what will happen rather than running the commands"
    };

    public static readonly Option<bool> SkipSchemeDownload = new("--skip-scheme-download")
    {
        Description = "Don't download the latest schemes before starting the game"
    };

    public static readonly Option<bool> SkipUpload = new("--skip-upload")
    {
        Description = "Don't Upload the replay to Worms Hub when the game finishes"
    };

    public static readonly Option<bool> SkipAnnouncement = new("--skip-announcement")
    {
        Description = "Don't announce the game to Slack or Worms Hub"
    };

    public Host()
        : base("host", "Host a game of worms using the latest options")
    {
        Options.Add(DryRun);
        Options.Add(SkipSchemeDownload);
        Options.Add(SkipUpload);
        Options.Add(SkipAnnouncement);
    }
}

internal sealed class HostHandler(
    IWormsArmageddon wormsArmageddon,
    LeagueUpdater leagueUpdater,
    IResourceCreator<RemoteGame, string> remoteGameCreator,
    IRemoteGameUpdater gameUpdater,
    IResourceRetriever<LocalReplay> localReplayRetriever,
    IResourceCreator<RemoteReplay, RemoteReplayCreateParameters> remoteReplayCreator,
    ILogger<HostHandler> logger) : AsynchronousCommandLineAction
{
    private const string LeagueName = "redgate";
    private const string Domain = "red-gate.com";

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Host.SpanName);

        var config = new Config(
            parseResult.GetValue(Host.DryRun),
            parseResult.GetValue(Host.SkipSchemeDownload),
            parseResult.GetValue(Host.SkipUpload),
            parseResult.GetValue(Host.SkipAnnouncement),
            GetIpAddress(Domain),
            wormsArmageddon.FindInstallation());

        RecordTelemetryForConfig(config);

        var validatedConfig = config.Validate(
            Valid.Rules<Config>()
                .Must(x => x.IpAddress.IsValid, x => x.IpAddress.Error.First())
                .Must(x => x.GameInfo.IsInstalled, "Worms Armageddon is not installed"));

        if (!validatedConfig.IsValid)
        {
            validatedConfig.LogErrors(logger);
            return 1;
        }

        await HostGame(validatedConfig.Value, cancellationToken);
        return 0;
    }

    private static void RecordTelemetryForConfig(Config config)
    {
        _ = Activity.Current?.AddTag(Telemetry.Spans.Host.DryRun, config.DryRun);
        _ = Activity.Current?.AddTag(Telemetry.Spans.Host.SkipSchemeDownload, config.SkipSchemeDownload);
        _ = Activity.Current?.AddTag(Telemetry.Spans.Host.SkipUpload, config.SkipUpload);
        _ = Activity.Current?.AddTag(Telemetry.Spans.Host.SkipAnnouncement, config.SkipAnnouncement);
        _ = Activity.Current?.AddTag(Telemetry.Spans.Host.IpAddressFound, config.IpAddress.IsValid);
        _ = Activity.Current?.AddTag(Telemetry.Spans.Host.WormsArmageddonIsInstalled, config.GameInfo.IsInstalled);
        _ = Activity.Current?.AddTag(Telemetry.Spans.Host.WormsArmageddonVersion, config.GameInfo.Version);
    }

    private async Task HostGame(Config config, CancellationToken cancellationToken)
    {
        await DownloadLatestOptions(config.SkipSchemeDownload, config.DryRun);
        var runGame = StartWorms(config.DryRun, cancellationToken);

        var game = await AnnounceGameToWormsHub(
            config.IpAddress.Value!,
            config.SkipAnnouncement,
            config.DryRun,
            cancellationToken);
        await WaitForGameToClose(runGame);
        await MarkGameCompleteOnWormsHub(game, config.SkipAnnouncement, config.DryRun, cancellationToken);
        await UploadReplayToWormsHub(config.SkipUpload, config.DryRun, cancellationToken);
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
            game = await remoteGameCreator.Create(hostIp, cancellationToken);
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
            await gameUpdater.SetGameComplete(game!, cancellationToken);
        }
    }

    private async Task UploadReplayToWormsHub(bool skipUpload, bool dryRun, CancellationToken cancellationToken)
    {
        if (skipUpload)
        {
            return;
        }

        logger.LogInformation("Uploading replay to hub");
        var allReplays = await localReplayRetriever.Retrieve(cancellationToken);
        var replay = allReplays.MaxBy(x => x.Details.Date);

        if (replay is null)
        {
            logger.LogWarning("No replay found to upload");
            return;
        }

        // Check if the replay was created during this session
        var timeSinceGameEnded = DateTime.UtcNow - replay.Details.Date.ToUniversalTime();
        if (timeSinceGameEnded > TimeSpan.FromHours(1))
        {
            logger.LogWarning("No recent replay found to upload");
            return;
        }

        logger.LogInformation("Uploading replay: {ReplayPath}", replay.Paths.WAgamePath);
        if (!dryRun)
        {
            _ = await remoteReplayCreator.Create(
                new RemoteReplayCreateParameters(replay.Details.Date.ToString("s"), replay.Paths.WAgamePath),
                cancellationToken);
        }
    }

    private static Validated<string> GetIpAddress(string domain)
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Bind(GetAdaptersForDomain())
            .Validate(NetworkAdapterExists())
            .Map(GetIpV4Address())
            .Validate(IpV4AddressExists())
            .Map(x => x!);

        Func<NetworkInterface[], NetworkInterface?> GetAdaptersForDomain() =>
            x => Array.Find(
                x,
                a => a.GetIPProperties().DnsSuffix == domain && a.OperationalStatus == OperationalStatus.Up);

        Func<NetworkInterface?, string?> GetIpV4Address() =>
            x => x!.GetIPProperties()
                .UnicastAddresses.FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork)
                ?.Address.ToString();

        List<ValidationRule<NetworkInterface?>> NetworkAdapterExists() =>
            Valid.Rules<NetworkInterface?>().Must(x => x is not null, $"No network adapter found for domain: {domain}");

        List<ValidationRule<string?>> IpV4AddressExists() =>
            Valid.Rules<string?>().Must(x => x is not null, $"No IPv4 address found for domain: {domain}");
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
        return !dryRun ? wormsArmageddon.Host() : Task.Delay(5000, cancellationToken);
    }

    private Task WaitForGameToClose(Task runGame)
    {
        logger.LogInformation("Waiting for game to finish");
        return runGame;
    }
}

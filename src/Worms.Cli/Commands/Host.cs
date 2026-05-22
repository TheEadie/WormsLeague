using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Commands.Validation;
using Worms.Cli.League;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Network;
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
    IIpAddressLookup ipAddressLookup,
    TimeProvider timeProvider,
    ILogger<HostHandler> logger) : AsynchronousCommandLineAction
{
    private const string LeagueName = "redgate";
    private const string Domain = "red-gate.com";

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Host.SpanName);

        Validated<string> ipAddress = ipAddressLookup.LookupForDomain(Domain) switch
        {
            IpAddressFound f => new Valid<string>(f.Address),
            IpAddressNotFound nf => new Invalid<string>(nf.Error),
            _ => throw new InvalidOperationException("Unexpected IpAddressLookupResult type")
        };

        var config = new Config(
            parseResult.GetValue(Host.DryRun),
            parseResult.GetValue(Host.SkipSchemeDownload),
            parseResult.GetValue(Host.SkipUpload),
            parseResult.GetValue(Host.SkipAnnouncement),
            ipAddress,
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
        var replay = allReplays.Where(x => x.HostedByLocalMachine).MaxBy(x => x.Details.Date);

        if (replay is null)
        {
            logger.LogWarning("No replay found to upload");
            _ = Activity.Current?.AddTag(Telemetry.Spans.Host.ReplayFound, false);
            return;
        }

        // Check if the replay was created during this session
        var timeSinceGameEnded = timeProvider.GetUtcNow().DateTime - replay.Details.Date;
        if (timeSinceGameEnded > TimeSpan.FromHours(1))
        {
            logger.LogWarning("No recent replay found to upload");
            _ = Activity.Current?.AddTag(Telemetry.Spans.Host.ReplayFound, false);
            _ = Activity.Current?.SetTag(Telemetry.Spans.Host.LatestReplayDate, replay.Details.Date);
            return;
        }

        logger.LogInformation("Uploading replay: {ReplayPath}", replay.Paths.WAgamePath);
        _ = Activity.Current?.AddTag(Telemetry.Spans.Host.ReplayFound, true);
        if (!dryRun)
        {
            _ = await remoteReplayCreator.Create(
                new RemoteReplayCreateParameters(replay.Details.Date.ToString("s"), replay.Paths.WAgamePath),
                cancellationToken);
        }
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
        return !dryRun ? wormsArmageddon.Host() : Task.Delay(TimeSpan.FromSeconds(5), timeProvider, cancellationToken);
    }

    private Task WaitForGameToClose(Task runGame)
    {
        logger.LogInformation("Waiting for game to finish");
        return runGame;
    }
}

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
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

internal sealed record HostOptions(
    bool DryRun,
    bool LocalMode,
    bool SkipSchemeDownload,
    bool SkipUpload,
    bool SkipAnnouncement)
{
    public static HostOptions FromContext(InvocationContext context) =>
        new(
            context.ParseResult.GetValueForOption(Host.DryRun),
            context.ParseResult.GetValueForOption(Host.LocalMode),
            context.ParseResult.GetValueForOption(Host.SkipSchemeDownload),
            context.ParseResult.GetValueForOption(Host.SkipUpload),
            context.ParseResult.GetValueForOption(Host.SkipAnnouncement));
}

internal sealed record HostConfig(
    bool DryRun,
    bool LocalMode,
    bool SkipSchemeDownload,
    bool SkipUpload,
    bool SkipAnnouncement,
    Validated<string> HostIp,
    GameInfo GameInfo);

[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
internal sealed class HostHandler(
    IWormsLocator wormsLocator,
    IWormsRunner wormsRunner,
    IConfigManager configManager,
    LeagueUpdater leagueUpdater,
    IResourceCreator<RemoteGame, string> remoteGameCreator,
    IRemoteGameUpdater gameUpdater,
    IResourceRetriever<LocalReplay> localReplayRetriever,
    IResourceCreator<RemoteReplay, RemoteReplayCreateParameters> remoteReplayCreator,
    ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        logger.Verbose("Loading configuration");
        var cancellationToken = context.GetCancellationToken();
        var options = HostOptions.FromContext(context);
        var configFile = configManager.Load();
        var hostIp = GetIpAddress("red-gate.com");
        var gameInfo = wormsLocator.Find();
        var hostConfig = ValidateConfig(
            new HostConfig(
                options.DryRun,
                options.LocalMode,
                options.SkipAnnouncement,
                options.SkipUpload,
                options.SkipSchemeDownload,
                hostIp,
                gameInfo));

        if (!hostConfig.IsValid)
        {
            hostConfig.LogErrors(logger);
            return 1;
        }

        var config = hostConfig.Value;

        await DownloadLatestOptions(config.SkipSchemeDownload, configFile, config.DryRun);
        var gameProcess = StartWorms(config.DryRun, cancellationToken);
        var gameDetails = await AnnounceGameToWormsHub(
            config.HostIp.Value!,
            config.SkipAnnouncement,
            config.DryRun,
            cancellationToken);
        await WaitForGameToClose(gameProcess);
        await MarkGameCompleteOnWormsHub(gameDetails, config.SkipAnnouncement, config.DryRun, cancellationToken);
        await UploadReplayToWormsHub(config.SkipUpload, config.DryRun, cancellationToken);
        return 0;
    }

    private static Validated<HostConfig> ValidateConfig(HostConfig config)
    {
        var errors = new List<string>();

        if (!config.HostIp.IsValid)
        {
            errors.Add("IP address could not be found");
            errors.AddRange(config.HostIp.Error);
        }

        if (!config.GameInfo.IsInstalled)
        {
            errors.Add("Worms Armageddon is not installed.");
        }

        return errors.Count != 0 ? new Invalid<HostConfig>(errors) : new Valid<HostConfig>(config);
    }

    private static Validated<string> GetIpAddress(string domain)
    {
        var adapters = NetworkInterface.GetAllNetworkInterfaces();
        var leagueNetworkAdapter = Array.Find(
            adapters,
            x => x.GetIPProperties().DnsSuffix == domain && x.OperationalStatus == OperationalStatus.Up);

        if (leagueNetworkAdapter == null)
        {
            return new Invalid<string>($"No network adapter for domain: {domain}");
        }

        var ipAddress = leagueNetworkAdapter.GetIPProperties()
            .UnicastAddresses.FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
            ?.Address.ToString();

        return ipAddress == null
            ? new Invalid<string>($"No IPv4 address found for network adapter: {leagueNetworkAdapter.Name}")
            : new Valid<string>(ipAddress);
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

        return !dryRun
            ? await remoteGameCreator.Create(hostIp, logger, cancellationToken).ConfigureAwait(false)
            : new RemoteGame("", "", "");
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
        return !dryRun ? wormsRunner.RunWorms("wa://") : Task.Delay(5000, cancellationToken);
    }

    private Task WaitForGameToClose(Task runGame)
    {
        logger.Information("Waiting for game to finish");
        return runGame;
    }
}

using System.Diagnostics;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Worms.Cli;

internal static class Telemetry
{
    private const string SourceName = "Worms.CLI";
    private const string HoneycombApiKey = "hcaik_01hxhm501wvw3agvzm04p5e9tgy55tjvpsafyj0y9f48rphyds7kag29a1";

    public static readonly ActivitySource Source = new(SourceName);

    public static readonly TracerProvider? TracerProvider =
        Environment.GetEnvironmentVariable("WORMS_DISABLE_TELEMETRY") is null
            ? Sdk.CreateTracerProviderBuilder()
                .AddSource(SourceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(
                            SourceName,
                            serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)))
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(option =>
                    {
                        option.Endpoint = new Uri("https://api.honeycomb.io");
                        option.Headers = $"x-honeycomb-team={HoneycombApiKey}";
                    })
                .Build()
            : null;

    internal static class Spans
    {
        internal static class Root
        {
            public const string SpanName = "worms";
            public const string ProcessExitCode = "process.exit.code";
            public const string Verbose = "worms.verbose";
            public const string Quiet = "worms.quiet";
            public const string UserId = "user.id";
            public const string LoggedIn = "user.logged_in";
        }

        internal static class Auth
        {
            public const string SpanName = "worms auth";
        }

        internal static class Version
        {
            public const string SpanName = "worms version";
            public const string CliVersion = "worms.version.cli_version";
            public const string WormsArmageddonVersion = "worms.version.worms_armageddon_version";
        }

        internal static class Update
        {
            public const string SpanName = "worms update";
            public const string Force = "worms.update.force";
            public const string LatestCliVersion = "worms.update.latest_cli_version";
            public const string UpdateFolderExists = "worms.update.update_folder_exists";
            public const string NumberOfFiles = "worms.update.number_of_files";
        }

        internal static class Host
        {
            public const string SpanName = "worms host";
            public const string DryRun = "worms.host.dry_run";
            public const string SkipSchemeDownload = "worms.host.skip_scheme_download";
            public const string SkipUpload = "worms.host.skip_upload";
            public const string SkipAnnouncement = "worms.host.skip_announcement";
            public const string IpAddressFound = "worms.host.ip_address_found";
            public const string WormsArmageddonIsInstalled = "worms.host.worms_armageddon_is_installed";
            public const string WormsArmageddonVersion = "worms.version.worms_armageddon_version";
            public const string ReplayFound = "worms.host.replay_found";
            public const string LatestReplayDate = "worms.host.latest_replay_date";
        }

        internal static class Game
        {
            public const string SpanNameGet = "worms get games";
        }

        internal static class Gif
        {
            public const string SpanNameBrowse = "worms browse gifs";
            public const string SpanNameCreate = "worms create gifs";
        }

        internal static class Replay
        {
            public const string SpanNameGet = "worms get replays";
            public const string SpanNameBrowse = "worms browse replays";
            public const string SpanNameDelete = "worms delete replays";
            public const string SpanNameProcess = "worms process replays";
            public const string SpanNameView = "worms view replays";
        }

        internal static class Scheme
        {
            public const string SpanNameGet = "worms get schemes";
            public const string SpanNameCreate = "worms create schemes";
            public const string SpanNameDelete = "worms delete schemes";
            public const string SpanNameBrowse = "worms browse schemes";

            public const string Id = "worms.scheme.id";
            public const string SchemeVersion = "worms.scheme.version";
        }

        internal static class League
        {
            public const string Id = "worms.league.id";
        }
    }

    internal static class Events
    {
        public static ActivityEvent DiSetupComplete = new("di.setup.complete");
        public static ActivityEvent TelemetrySetupComplete = new("telemetry.setup.complete");
    }
}

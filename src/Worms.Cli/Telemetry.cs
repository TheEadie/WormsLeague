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
                .AddOtlpExporter(
                    option =>
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
            public const string LeagueId = "worms.host.league_id";
            public const string SchemeVersion = "worms.host.scheme_version";
        }

        internal static class League
        {
            public const string Id = "worms.league.id";
        }

        internal static class Scheme
        {
            public const string Id = "worms.scheme.id";
            public const string Version = "worms.scheme.version";
        }
    }

    internal static class Events
    {
        public static ActivityEvent DiSetupComplete = new("di.setup.complete");
        public static ActivityEvent TelemetrySetupComplete = new("telemetry.setup.complete");
    }
}

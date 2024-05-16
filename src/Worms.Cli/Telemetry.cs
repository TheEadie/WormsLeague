using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    public static readonly TracerProvider? TracerProvider = Sdk.CreateTracerProviderBuilder()
        .AddSource(SourceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(SourceName, serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)))
        .AddConsoleExporter()
        .AddOtlpExporter(
            option =>
                {
                    option.Endpoint = new Uri("https://api.honeycomb.io");
                    option.Headers = $"x-honeycomb-team={HoneycombApiKey}";
                })
        .Build();

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Using underscores to namespace attributes")]
    internal static class Attributes
    {
        public const string Name = "name";
        public const string Process_Exit_Code = "process.exit.code";
        public const string Version_CliVersion = "worms.version.cli_version";
        public const string Version_WormsArmageddonVersion = "worms.version.worms_armageddon_version";
    }
}

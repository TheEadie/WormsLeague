using System.Diagnostics;
using System.Reflection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Worms.Hub.Armageddon.Runner;

internal static class Telemetry
{
    private const string SourceName = "Worms.Hub.WA.Runner";
    private const string HoneycombApiKey = "hcaik_01hz7eat48jq3fz3vgg6mred9gzc5pfbgpkx5fvgtbj33hc4sbzpgs1e1h";

    public static readonly ActivitySource Source = new(SourceName);

    public static void AddOpenTelemetryWaRunner(this IServiceCollection services)
    {
        _ = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                SourceName,
                serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)))
            .WithTracing(tracing => tracing.AddOtlpExporter(option =>
                {
                    option.Endpoint = new Uri("https://api.honeycomb.io");
                    option.Headers = $"x-honeycomb-team={HoneycombApiKey}";
                }));
    }
}

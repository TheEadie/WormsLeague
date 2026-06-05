using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Armageddon.Gifs;
using Worms.Cli.Logging;
using Worms.Cli.Resources;

namespace Worms.Cli;

internal static class Program
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    public static async Task<int> Main(string[] args)
    {
        var startTime = DateTime.UtcNow;
        using var tracerProvider = Telemetry.TracerProvider;
        using var span = Telemetry.Source.StartActivity(
            ActivityKind.Server,
            startTime: startTime,
            name: Telemetry.Spans.Root.SpanName);
        _ = span?.AddEvent(Telemetry.Events.TelemetrySetupComplete);

        var configuration = new ConfigurationBuilder().AddEnvironmentVariables("WORMS_").Build();

        var logLevel = LogLevelParser.GetLogLevel(args);
        _ = span?.SetTag(Telemetry.Spans.Root.Verbose, logLevel == LogLevel.Debug);
        _ = span?.SetTag(Telemetry.Spans.Root.Quiet, logLevel == LogLevel.Error);

        var serviceCollection = new ServiceCollection().AddHttpClient()
            .AddWormsCliLogging(logLevel)
            .AddWormsArmageddonFilesServices()
            .AddWormsArmageddonGameServices()
            .AddWormsArmageddonGifsServices()
            .AddWormsCliResourcesServices()
            .AddWormsCliServices()
            .AddSingleton<IConfiguration>(configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var rootCommand = CliStructure.BuildCommandLine(serviceProvider);

        _ = span?.AddEvent(Telemetry.Events.DiSetupComplete);

        var runner = serviceProvider.GetRequiredService<Runner>();
        var result = await runner.Run(rootCommand, args, CancellationToken.None);

        _ = span?.SetTag(Telemetry.Spans.Root.ProcessExitCode, result);
        return result;
    }

    [UnconditionalSuppressMessage("RequiresDynamicCodeEvaluation", "IL3050")]
    [UnconditionalSuppressMessage("RequiresDynamicCodeEvaluation", "IL2026")]
    private static IServiceCollection AddWormsCliLogging(this IServiceCollection builder, LogLevel logLevel) =>
        builder.AddLogging(
            loggingBuilder =>
                {
                    _ = loggingBuilder.AddConsole(
                            options =>
                                {
                                    options.LogToStandardErrorThreshold = LogLevel.Trace;
                                    options.FormatterName = nameof(ColorFormatter);
                                })
                        .AddConsoleFormatter<ColorFormatter, ColorLoggerOptions>(
                            options => options.ColorBehavior = LoggerColorBehavior.Enabled)
                        .AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.None)
                        .AddFilter<ConsoleLoggerProvider>("System", LogLevel.None)
                        .AddFilter<ConsoleLoggerProvider>("Worms", logLevel);
                });
}

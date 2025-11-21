using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Cli.Logging;
using Worms.Cli.Resources;
using System.Text.Json;

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

        var serviceCollection = new ServiceCollection().AddHttpClient()
            .AddWormsCliLogging(GetLogLevel(args))
            .AddWormsArmageddonFilesServices()
            .AddWormsArmageddonGameServices()
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

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "This is more readable")]
    private static LogLevel GetLogLevel(string[] args)
    {
        var verbose = args.Contains("-v") || args.Contains("--verbose");
        var quiet = args.Contains("-q") || args.Contains("--quiet");

        _ = Activity.Current?.SetTag(Telemetry.Spans.Root.Verbose, verbose);
        _ = Activity.Current?.SetTag(Telemetry.Spans.Root.Quiet, quiet);

        if (verbose)
        {
            return LogLevel.Debug;
        }

        if (quiet)
        {
            return LogLevel.Error;
        }

        return LogLevel.Information;
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

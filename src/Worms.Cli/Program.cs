using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry.Trace;
using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Cli.Logging;
using Worms.Cli.Resources;

namespace Worms.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var startTime = DateTime.UtcNow;
        using var tracerProvider = Environment.GetEnvironmentVariable("WORMS_DISABLE_TELEMETRY") is null
            ? Telemetry.TracerProvider
            : null;
        using var span = Telemetry.Source.StartActivity(
            ActivityKind.Server,
            startTime: startTime,
            name: Telemetry.Spans.Root.SpanName);
        _ = span?.AddEvent(Telemetry.Events.TelemetrySetupComplete);

        var serviceCollection = new ServiceCollection().AddHttpClient()
            .AddWormsCliLogging(GetLogLevel(args))
            .AddWormsArmageddonFilesServices()
            .AddWormsArmageddonGameServices()
            .AddWormsCliResourcesServices()
            .AddWormsCliServices();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _ = span?.AddEvent(Telemetry.Events.DiSetupComplete);

        var result = await CliStructure.BuildCommandLine(serviceProvider)
            .UseDefaults()
            .UseExceptionHandler(ExceptionHandler)
            .Build()
            .InvokeAsync(args)
            .ConfigureAwait(false);

        _ = span?.SetTag(Telemetry.Spans.Root.ProcessExitCode, result);

        return result!;
    }

    private static void ExceptionHandler(Exception exception, InvocationContext context)
    {
        if (exception is not OperationCanceledException)
        {
            context.Console.Error.Write(context.LocalizationResources.ExceptionHandlerHeader());
            context.Console.Error.WriteLine(exception.Message);

            Activity.Current?.RecordException(exception);
            _ = Activity.Current?.SetStatus(ActivityStatusCode.Error);
        }

        context.ExitCode = 1;
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

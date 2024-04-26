using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Cli.Logging;
using Worms.Cli.Resources;

namespace Worms.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var serviceCollection = new ServiceCollection().AddHttpClient()
            .AddWormsCliLogging(GetLogLevel(args))
            .AddWormsArmageddonFilesServices()
            .AddWormsArmageddonGameServices()
            .AddWormsCliResourcesServices()
            .AddWormsCliServices();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return await CliStructure.BuildCommandLine(serviceProvider)
            .UseDefaults()
            .Build()
            .InvokeAsync(args)
            .ConfigureAwait(false);
    }

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "This is more readable")]
    private static LogLevel GetLogLevel(string[] args)
    {
        if (args.Contains("-v") || args.Contains("--verbose"))
        {
            return LogLevel.Debug;
        }

        if (args.Contains("-q") || args.Contains("--quiet"))
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

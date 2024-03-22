using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Cli.Logging;
using Worms.Cli.Resources;

namespace Worms.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var logger = new LoggerConfiguration().MinimumLevel.Is(GetLogEventLevel(args))
            .WriteTo.ColoredConsole(formatProvider: CultureInfo.CurrentCulture)
            .CreateLogger();

        var serviceCollection = new ServiceCollection().AddHttpClient()
            .AddWormsArmageddonFilesServices()
            .AddWormsArmageddonGameServices()
            .AddWormsCliResourcesServices()
            .AddWormsCliServices()
            .AddScoped<ILogger>(_ => logger);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return await CliStructure.BuildCommandLine(serviceProvider)
            .UseDefaults()
            .Build()
            .InvokeAsync(args)
            .ConfigureAwait(false);
    }

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "This is more readable")]
    private static LogEventLevel GetLogEventLevel(string[] args)
    {
        if (args.Contains("-v") || args.Contains("--verbose"))
        {
            return LogEventLevel.Verbose;
        }

        if (args.Contains("-q") || args.Contains("--quiet"))
        {
            return LogEventLevel.Error;
        }

        return LogEventLevel.Information;
    }
}

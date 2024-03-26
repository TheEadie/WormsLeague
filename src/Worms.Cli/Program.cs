using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Files;
using Worms.Armageddon.Game;
using Worms.Cli.Resources;

namespace Worms.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var serviceCollection = new ServiceCollection().AddHttpClient()
            .AddLogging(
                builder =>
                    {
                        _ = builder.SetMinimumLevel(GetLogLevel(args))
                            .AddSimpleConsole(options => options.SingleLine = true);
                    })
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
}

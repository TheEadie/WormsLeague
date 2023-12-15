using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Globalization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Worms.Cli.Logging;
using Worms.Cli.Modules;

namespace Worms.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var serviceProvider = GetServiceProvider(args);
        var runner = CliStructure.BuildCommandLine(serviceProvider).UseDefaults().Build();

        var result = await runner.InvokeAsync(args).ConfigureAwait(false);

        await serviceProvider.DisposeAsync().ConfigureAwait(false);
        return result;
    }

    private static AutofacServiceProvider GetServiceProvider(string[] args)
    {
        var logger = new LoggerConfiguration().MinimumLevel.Is(GetLogEventLevel(args))
            .WriteTo.ColoredConsole(formatProvider: CultureInfo.CurrentCulture)
            .CreateLogger();

        var serviceCollection = new ServiceCollection().AddHttpClient();
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(serviceCollection);
        _ = containerBuilder.RegisterModule<CliModule>();
        _ = containerBuilder.RegisterInstance<ILogger>(logger);
        var container = containerBuilder.Build();
        return new AutofacServiceProvider(container);
    }

    private static LogEventLevel GetLogEventLevel(string[] args) =>
        args.Contains("-v") || args.Contains("--verbose") ? LogEventLevel.Verbose :
        args.Contains("-q") || args.Contains("--quiet") ? LogEventLevel.Error : LogEventLevel.Information;
}

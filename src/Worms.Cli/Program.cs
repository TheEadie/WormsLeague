using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Globalization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Worms.Cli.Commands;
using Worms.Cli.Commands.Resources.Games;
using Worms.Cli.Commands.Resources.Gifs;
using Worms.Cli.Commands.Resources.Replays;
using Worms.Cli.Commands.Resources.Schemes;
using Worms.Cli.Logging;
using Worms.Cli.Modules;
using Version = Worms.Cli.Commands.Version;
using Host = Worms.Cli.Commands.Host;

namespace Worms.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var logger = new LoggerConfiguration().MinimumLevel.Is(GetLogEventLevel(args))
            .WriteTo.ColoredConsole(formatProvider: CultureInfo.CurrentCulture)
            .CreateLogger();

        var runner = CliStructure.BuildCommandLine()
            .UseHost(
                _ => new HostBuilder(),
                (builder) =>
                    {
                        _ = builder.ConfigureServices(x => x.AddHttpClient());
                        _ = builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                            .ConfigureContainer<ContainerBuilder>(
                                container =>
                                    {
                                        _ = container.RegisterModule<CliModule>();
                                        _ = container.RegisterInstance<ILogger>(logger);
                                    })
                            .UseCommandHandler<Auth, AuthHandler>()
                            .UseCommandHandler<Version, VersionHandler>()
                            .UseCommandHandler<Update, UpdateHandler>()
                            .UseCommandHandler<Setup, SetupHandler>()
                            .UseCommandHandler<Host, HostHandler>()
                            .UseCommandHandler<ViewReplay, ViewReplayHandler>()
                            .UseCommandHandler<ProcessReplay, ProcessReplayHandler>()
                            .UseCommandHandler<GetScheme, GetSchemeHandler>()
                            .UseCommandHandler<GetReplay, GetReplayHandler>()
                            .UseCommandHandler<GetGame, GetGameHandler>()
                            .UseCommandHandler<DeleteScheme, DeleteSchemeHandler>()
                            .UseCommandHandler<DeleteReplay, DeleteReplayHandler>()
                            .UseCommandHandler<CreateScheme, CreateSchemeHandler>()
                            .UseCommandHandler<CreateGif, CreateGifHandler>()
                            .UseCommandHandler<BrowseScheme, BrowseSchemeHandler>()
                            .UseCommandHandler<BrowseReplay, BrowseReplayHandler>()
                            .UseCommandHandler<BrowseGif, BrowseGifHandler>();
                    })
            .UseDefaults()
            .Build();

        return await runner.InvokeAsync(args);
    }


    private static LogEventLevel GetLogEventLevel(string[] args) =>
        args.Contains("-v") || args.Contains("--verbose") ? LogEventLevel.Verbose :
        args.Contains("-q") || args.Contains("--quiet") ? LogEventLevel.Error : LogEventLevel.Information;
}

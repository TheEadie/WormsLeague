using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Autofac;
using Autofac.Extensions.DependencyInjection;
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

namespace Worms.Cli
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var logger = new LoggerConfiguration().MinimumLevel.Is(GetLogEventLevel(args))
                .WriteTo.ColoredConsole()
                .CreateLogger();

            var runner = CliStructure.BuildCommandLine()
                .UseHost(
                    _ => new HostBuilder(),
                    (builder) =>
                        {
                            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                                .ConfigureContainer<ContainerBuilder>(
                                    container =>
                                        {
                                            container.RegisterModule<CliModule>();
                                            container.RegisterInstance<ILogger>(logger);
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
}

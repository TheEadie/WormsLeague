using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Worms.Commands;
using Worms.Commands.Resources;
using Worms.Commands.Resources.Games;
using Worms.Commands.Resources.Replays;
using Worms.Commands.Resources.Schemes;
using Worms.Logging;
using Worms.Modules;
using Version = Worms.Commands.Version;

namespace Worms
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(GetLogEventLevel(args))
                .WriteTo.ColoredConsole()
                .CreateLogger();

            var runner = BuildCommandLine()
                .UseHost(_ => new HostBuilder(), (builder) =>
                {
                    builder
                        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                        .ConfigureContainer<ContainerBuilder>(container =>
                        {
                            container.RegisterModule<CliModule>();
                            container.RegisterInstance<ILogger>(logger);
                        })
                        .UseCommandHandler<Auth, AuthHandler>()
                        .UseCommandHandler<Version, VersionHandler>()
                        .UseCommandHandler<Update, UpdateHandler>()
                        .UseCommandHandler<Setup, SetupHandler>()
                        .UseCommandHandler<Commands.Host, HostHandler>()
                        .UseCommandHandler<ViewReplay, ViewReplayHandler>()
                        .UseCommandHandler<ProcessReplay, ProcessReplayHandler>()
                        .UseCommandHandler<GetScheme, GetSchemeHandler>()
                        .UseCommandHandler<GetReplay, GetReplayHandler>()
                        .UseCommandHandler<GetGame, GetGameHandler>()
                        .UseCommandHandler<DeleteScheme, DeleteSchemeHandler>()
                        .UseCommandHandler<DeleteReplay, DeleteReplayHandler>();
                })
                .UseDefaults()
                .Build();

            return await runner.InvokeAsync(args);
        }

        private static CommandLineBuilder BuildCommandLine()
        {
            var rootCommand = new Root();
            rootCommand.AddCommand(new Auth());
            rootCommand.AddCommand(new Version());
            rootCommand.AddCommand(new Update());
            rootCommand.AddCommand(new Setup());
            rootCommand.AddCommand(new Commands.Host());

            var viewCommand = new View();
            viewCommand.AddCommand(new ViewReplay());
            rootCommand.AddCommand(viewCommand);

            var processCommand = new Process();
            processCommand.AddCommand(new ProcessReplay());
            rootCommand.AddCommand(processCommand);

            var getCommand = new Get();
            getCommand.AddCommand(new GetScheme());
            getCommand.AddCommand(new GetReplay());
            getCommand.AddCommand(new GetGame());
            rootCommand.AddCommand(getCommand);

            var deleteCommand = new Delete();
            deleteCommand.AddCommand(new DeleteScheme());
            deleteCommand.AddCommand(new DeleteReplay());
            rootCommand.AddCommand(deleteCommand);

            return new CommandLineBuilder(rootCommand);
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
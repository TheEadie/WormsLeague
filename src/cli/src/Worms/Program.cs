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
                        .UseCommandHandler<Commands.Host, HostHandler>();
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

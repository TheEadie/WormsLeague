using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli;

namespace Worms.Commands
{
    internal class Version : Command
    {
        public Version() : base("version", "Get the current version of the Worms CLI") { }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class VersionHandler : ICommandHandler
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly CliInfoRetriever _cliInfoRetriever;
        private readonly ILogger _logger;

        public VersionHandler(IWormsLocator wormsLocator, CliInfoRetriever cliInfoRetriever, ILogger logger)
        {
            _wormsLocator = wormsLocator;
            _cliInfoRetriever = cliInfoRetriever;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) => 
            Task.Run(async () => await InvokeAsync(context)).Result;

        public Task<int> InvokeAsync(InvocationContext context)
        {
            var cliInfo = _cliInfoRetriever.Get();
            _logger.Information($"Worms CLI: {cliInfo.Version.ToString(3)}");

            var gameInfo = _wormsLocator.Find();
            var gameVersion = gameInfo.IsInstalled ? gameInfo.Version.ToString(4) : "Not Installed";
            _logger.Information($"Worms Armageddon: {gameVersion}");
            return Task.FromResult(0);
        }
    }
}

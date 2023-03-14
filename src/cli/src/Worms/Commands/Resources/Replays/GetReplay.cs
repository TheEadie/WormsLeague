using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Resources.Local.Replays;
using Worms.Resources;

namespace Worms.Commands.Resources.Replays
{
    internal class GetReplay : Command
    {
        public static readonly Argument<string> ReplayName =
            new("name",
                "Optional: The name or search pattern for the Replay to be retrieved. Wildcards (*) are supported");

        public GetReplay() :
            base("replay",
                "Retrieves information for Worms replays (.WAgame files)")
        {
            AddAlias("replays");
            AddAlias("WAGame");
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GetReplayHandler : ICommandHandler
    {
        private readonly ResourceGetter<LocalReplay> _replayRetriever;
        private readonly ILogger _logger;

        public GetReplayHandler(ResourceGetter<LocalReplay> replayRetriever, ILogger logger)
        {
            _replayRetriever = replayRetriever;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var name = context.ParseResult.GetValueForArgument(GetReplay.ReplayName);
            var cancellationToken = context.GetCancellationToken();

            try
            {
                var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
                await _replayRetriever.PrintResources(name, Console.Out, windowWidth, _logger,
                    cancellationToken);
            }
            catch (ConfigurationException exception)
            {
                _logger.Error(exception.Message);
                return 1;
            }

            return 0;
        }
    }
}
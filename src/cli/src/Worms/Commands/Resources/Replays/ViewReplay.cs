using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Resources.Local.Replays;
using Worms.Resources;

namespace Worms.Commands.Resources.Replays
{
    internal class ViewReplay : Command
    {
        public static readonly Argument<string> ReplayName = new("name", 
            "The name of the Replay to be viewed");

        public static readonly Option<uint> Turn = new(
            new[] {"--turn", "-t"},
            "The turn you wish to start the replay from");
        
        public ViewReplay() : base("replay", "View replays (.WAgame file)")
        {
            AddAlias("replays");
            AddAlias("WAgame");
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ViewReplayHandler : ICommandHandler
    {
        private readonly ResourceViewer<LocalReplay, LocalReplayViewParameters> _resourceViewer;
        private readonly ILogger _logger;

        public ViewReplayHandler(
            ResourceViewer<LocalReplay, LocalReplayViewParameters> resourceViewer,
            ILogger logger)
        {
            _resourceViewer = resourceViewer;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var name = context.ParseResult.GetValueForArgument(ViewReplay.ReplayName);
            var turn = context.ParseResult.GetValueForOption(ViewReplay.Turn);
            
            try
            {
                await _resourceViewer.View(name,
                    new LocalReplayViewParameters(turn),
                    _logger,
                    context.GetCancellationToken());
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
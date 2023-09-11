using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Replays
{
    internal class DeleteReplay : Command
    {
        public static readonly Argument<string> ReplayName =
            new("name", "The name of the Replay to be deleted");

        public DeleteReplay() : base("replay", "Delete replays (.WAgame files)")
        {
            AddAlias("replays");
            AddAlias("WAgame");
            AddArgument(ReplayName);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DeleteReplayHandler : ICommandHandler
    {
        private readonly ResourceDeleter<LocalReplay> _resourceDeleter;
        private readonly ILogger _logger;

        public DeleteReplayHandler(ResourceDeleter<LocalReplay> resourceDeleter, ILogger logger)
        {
            _resourceDeleter = resourceDeleter;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var name = context.ParseResult.GetValueForArgument(DeleteReplay.ReplayName);
            var cancellationToken = context.GetCancellationToken();

            try
            {
                await _resourceDeleter.Delete(name, _logger, cancellationToken);
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

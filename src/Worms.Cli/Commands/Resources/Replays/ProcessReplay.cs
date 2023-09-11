using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Serilog;
using Worms.Armageddon.Game.Replays;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Commands.Resources.Replays
{
    internal class ProcessReplay : Command
    {
        public static readonly Argument<string> ReplayName = new("name",
            () => "",
            "Optional: The name or search pattern for the Replay to be processed. Wildcards (*) are supported");

        public ProcessReplay() : base("replay", "Extract more information from replays (.WAgame files)")
        {
            AddAlias("replays");
            AddAlias("WAgame");
            AddArgument(ReplayName);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ProcessReplayHandler : ICommandHandler
    {
        private readonly IReplayLogGenerator _replayLogGenerator;
        private readonly IResourceRetriever<LocalReplay> _replayRetriever;
        private readonly ILogger _logger;

        public ProcessReplayHandler(IReplayLogGenerator replayLogGenerator,
            IResourceRetriever<LocalReplay> replayRetriever,
            ILogger logger)
        {
            _replayLogGenerator = replayLogGenerator;
            _replayRetriever = replayRetriever;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var name = context.ParseResult.GetValueForArgument(ProcessReplay.ReplayName);
            var cancellationToken = context.GetCancellationToken();

            var pattern = string.Empty;

            if (name != "*" && !string.IsNullOrEmpty(name))
            {
                pattern = name;
            }

            foreach (var replayPath in await _replayRetriever.Get(pattern, _logger, cancellationToken))
            {
                _logger.Information($"Processing: {replayPath.Paths.WAgamePath}");
                await _replayLogGenerator.GenerateReplayLog(replayPath.Paths.WAgamePath);
            }

            return 0;
        }
    }
}

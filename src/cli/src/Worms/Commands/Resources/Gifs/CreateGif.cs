using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Gifs;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Commands.Resources.Gifs
{
    internal class CreateGif : Command
    {
        public static readonly Option<string> ReplayName = new(
            new[] {"--replay", "-r"},
            "The replay name");

        public static readonly Option<uint> Turn = new(
            new[] {"--turn", "-t"},
            "The turn number");

        public static readonly Option<uint> FramesPerSecond = new(
            new[] {"--frames-per-second", "-fps"},
            () => 5,
            "The number of frames per second");

        public static readonly Option<uint> Speed = new(
            new[] {"--speed", "-s"},
            () => 2,
            "Speed multiplier for the gif");

        public static readonly Option<uint> StartOffset = new(
            new[] {"--start-offset", "-so"},
            () => 0,
            "Offset for the start of the gif in seconds");

        public static readonly Option<uint> EndOffset = new(
            new[] {"--end-offset", "-eo"},
            () => 0,
            "Offset for the end of the gif in seconds");

        public CreateGif() : base("gif", "Create animated gifs of replays (.gif files)")
        {
            AddOption(ReplayName);
            AddOption(Turn);
            AddOption(FramesPerSecond);
            AddOption(Speed);
            AddOption(StartOffset);
            AddOption(EndOffset);
        }
    }

    internal class CreateGifHandler : ICommandHandler
    {
        private readonly IResourceCreator<LocalGif, LocalGifCreateParameters> _gifCreator;
        private readonly IResourceRetriever<LocalReplay> _replayRetriever;
        private readonly ILogger _logger;

        public CreateGifHandler(IResourceCreator<LocalGif, LocalGifCreateParameters> gifCreator,
            IResourceRetriever<LocalReplay> replayRetriever,
            ILogger logger)
        {
            _gifCreator = gifCreator;
            _replayRetriever = replayRetriever;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var replayName = context.ParseResult.GetValueForOption(CreateGif.ReplayName);
            var turn = context.ParseResult.GetValueForOption(CreateGif.Turn);
            var fps = context.ParseResult.GetValueForOption(CreateGif.FramesPerSecond);
            var speed = context.ParseResult.GetValueForOption(CreateGif.Speed);
            var startOffset = context.ParseResult.GetValueForOption(CreateGif.StartOffset);
            var endOffset = context.ParseResult.GetValueForOption(CreateGif.EndOffset);
            var cancellationToken = context.GetCancellationToken();

            LocalReplay replay;

            try
            {
                replay = await ValidateReplay(replayName, turn, cancellationToken);
            }
            catch (ConfigurationException exception)
            {
                _logger.Error(exception.Message);
                return 1;
            }

            try
            {
                _logger.Information($"Creating gif for {replayName}, turn {turn} ...");
                var gif = await _gifCreator.Create(new LocalGifCreateParameters(replay, turn,
                    TimeSpan.FromSeconds(startOffset), TimeSpan.FromSeconds(endOffset),
                    fps, speed), _logger, cancellationToken);
                await Console.Out.WriteLineAsync(gif.Path);
            }
            catch (FormatException exception)
            {
                _logger.Error("Failed to create gif: " + exception.Message);
                return 1;
            }

            return 0;
        }

        private async Task<LocalReplay> ValidateReplay(string replay, uint turn, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(replay))
            {
                throw new ConfigurationException("No replay provided for the Gif being created");
            }

            if (turn == default)
            {
                throw new ConfigurationException("No turn provided for the Gif being created");
            }

            var replays = await _replayRetriever.Get(replay, _logger, cancellationToken);

            switch (replays.Count)
            {
                case 0:
                    throw new ConfigurationException($"No replays found with name: {replay}");
                case > 1:
                    throw new ConfigurationException($"More than one replay found matching pattern: {replay}");
            }

            var foundReplay = replays.Single();
            if (foundReplay.Details.Turns.Count < turn)
            {
                throw new ConfigurationException($"Replay {replay} does not have a turn {turn}");
            }

            return foundReplay;
        }
    }
}
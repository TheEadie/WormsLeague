using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Gifs;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Gifs;

internal sealed class CreateGif : Command
{
    public static readonly Option<string> ReplayName = new(
        [
            "--replay",
            "-r"
        ],
        "The replay name");

    public static readonly Option<uint> Turn = new(
        [
            "--turn",
            "-t"
        ],
        "The turn number");

    public static readonly Option<uint> FramesPerSecond = new(
        [
            "--frames-per-second",
            "-fps"
        ],
        () => 5,
        "The number of frames per second");

    public static readonly Option<uint> Speed = new(
        [
            "--speed",
            "-s"
        ],
        () => 2,
        "Speed multiplier for the gif");

    public static readonly Option<uint> StartOffset = new(
        [
            "--start-offset",
            "-so"
        ],
        () => 0,
        "Offset for the start of the gif in seconds");

    public static readonly Option<uint> EndOffset = new(
        [
            "--end-offset",
            "-eo"
        ],
        () => 0,
        "Offset for the end of the gif in seconds");

    public CreateGif()
        : base("gif", "Create animated gifs of replays (.gif files)")
    {
        AddOption(ReplayName);
        AddOption(Turn);
        AddOption(FramesPerSecond);
        AddOption(Speed);
        AddOption(StartOffset);
        AddOption(EndOffset);
    }
}

internal sealed class CreateGifHandler(
    IResourceCreator<LocalGif, LocalGifCreateParameters> gifCreator,
    IResourceRetriever<LocalReplay> replayRetriever,
    ILogger<CreateGifHandler> logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

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
            replay = await ValidateReplay(replayName, turn, cancellationToken).ConfigureAwait(false);
        }
        catch (ConfigurationException exception)
        {
            logger.LogError("{Message}", exception.Message);
            return 1;
        }

        try
        {
            logger.LogInformation("Creating gif for {ReplayName}, turn {Turn} ...", replayName, turn);
            var gif = await gifCreator.Create(
                    new LocalGifCreateParameters(
                        replay,
                        turn,
                        TimeSpan.FromSeconds(startOffset),
                        TimeSpan.FromSeconds(endOffset),
                        fps,
                        speed),
                    cancellationToken)
                .ConfigureAwait(false);
            await Console.Out.WriteLineAsync(gif.Path).ConfigureAwait(false);
        }
        catch (FormatException exception)
        {
            logger.LogError("Failed to create gif: {Message}", exception.Message);
            return 1;
        }

        return 0;
    }

    private async Task<LocalReplay> ValidateReplay(string? replay, uint turn, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(replay))
        {
            throw new ConfigurationException("No replay provided for the Gif being created");
        }

        if (turn == default)
        {
            throw new ConfigurationException("No turn provided for the Gif being created");
        }

        var replays = await replayRetriever.Retrieve(replay, cancellationToken).ConfigureAwait(false);

        if (replays.Count == 0)
        {
            throw new ConfigurationException($"No replays found with name: {replay}");
        }

        if (replays.Count > 1)
        {
            throw new ConfigurationException($"More than one replay found matching pattern: {replay}");
        }

        var foundReplay = replays.Single();
        return foundReplay.Details.Turns.Count < turn
            ? throw new ConfigurationException($"Replay {replay} does not have a turn {turn}")
            : foundReplay;
    }
}

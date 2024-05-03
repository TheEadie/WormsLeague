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
        new[]
        {
            "--replay",
            "-r"
        },
        "The replay name");

    public static readonly Option<uint> Turn = new(
        new[]
        {
            "--turn",
            "-t"
        },
        "The turn number");

    public static readonly Option<uint> FramesPerSecond = new(
        new[]
        {
            "--frames-per-second",
            "-fps"
        },
        () => 5,
        "The number of frames per second");

    public static readonly Option<uint> Speed = new(
        new[]
        {
            "--speed",
            "-s"
        },
        () => 2,
        "Speed multiplier for the gif");

    public static readonly Option<uint> StartOffset = new(
        new[]
        {
            "--start-offset",
            "-so"
        },
        () => 0,
        "Offset for the start of the gif in seconds");

    public static readonly Option<uint> EndOffset = new(
        new[]
        {
            "--end-offset",
            "-eo"
        },
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

        var config = new Config(replayName, turn, fps, speed, startOffset, endOffset);
        var validatedConfig = config.Validate(
        [
            (x => string.IsNullOrWhiteSpace(x.ReplayName), "No replay provided for the Gif being created"),
            (x => x.Turn == default, "No turn provided for the Gif being created")
        ]);

        var replays = await GetReplaysForPattern(validatedConfig, cancellationToken).ConfigureAwait(false);
        replays = replays.Validate(
            new List<(Func<List<LocalReplay>, bool> predicate, string error)>
            {
                new(x => x.Count == 0, $"No replays found with name: {replayName}"),
                new(x => x.Count > 1, $"More than one replay found matching pattern: {replayName}")
            });

        var foundReplay = GetSingleReplay(replays);

        var replay = foundReplay.Validate(
            new List<(Func<LocalReplay, bool>, string)>
            {
                new(
                    x => x.Details.Turns.Count < turn,
                    $"Replay {replayName} only has {foundReplay.Value!.Details.Turns.Count} turns, cannot create gif for turn {turn}")
            });

        if (!replay.IsValid)
        {
            replay.LogErrors(logger);
            return 1;
        }

        logger.LogInformation("Creating gif for {ReplayName}, turn {Turn} ...", replayName, turn);
        var gif = await gifCreator.Create(
                new LocalGifCreateParameters(
                    replay.Value,
                    turn,
                    TimeSpan.FromSeconds(startOffset),
                    TimeSpan.FromSeconds(endOffset),
                    fps,
                    speed),
                cancellationToken)
            .ConfigureAwait(false);
        await Console.Out.WriteLineAsync(gif.Path).ConfigureAwait(false);
        return 0;
    }

    private static Validated<LocalReplay> GetSingleReplay(Validated<List<LocalReplay>> replays) =>
        !replays.IsValid ? new Invalid<LocalReplay>(replays.Error) : new Valid<LocalReplay>(replays.Value!.Single());

    private async Task<Validated<List<LocalReplay>>> GetReplaysForPattern(
        Validated<Config> config,
        CancellationToken cancellationToken) =>
        !config.IsValid
            ? new Invalid<List<LocalReplay>>(config.Error)
            : new Valid<List<LocalReplay>>(
                (await replayRetriever.Retrieve(config.Value!.ReplayName!, cancellationToken).ConfigureAwait(false))
                .ToList());

    private sealed record Config(
        string? ReplayName,
        uint Turn,
        uint FramesPerSecond,
        uint Speed,
        uint StartOffset,
        uint EndOffset);
}

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Cli.Commands.Validation;
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
        Task.Run(async () => await InvokeAsync(context)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Gif.SpanNameCreate);
        var replayName = context.ParseResult.GetValueForOption(CreateGif.ReplayName);
        var turn = context.ParseResult.GetValueForOption(CreateGif.Turn);
        var fps = context.ParseResult.GetValueForOption(CreateGif.FramesPerSecond);
        var speed = context.ParseResult.GetValueForOption(CreateGif.Speed);
        var startOffset = context.ParseResult.GetValueForOption(CreateGif.StartOffset);
        var endOffset = context.ParseResult.GetValueForOption(CreateGif.EndOffset);
        var cancellationToken = context.GetCancellationToken();

        var config = new Config(replayName, turn, fps, speed, startOffset, endOffset);
        var replay = await config.Validate(ValidConfig())
            .Map(x => FindReplays(x.ReplayName!, cancellationToken))
            .Validate(Only1ReplayFound(replayName))
            .Map(x => x.Single())
            .Validate(ValidConfigForReplay(replayName));

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
            cancellationToken);
        await Console.Out.WriteLineAsync(gif.Path);
        return 0;
    }

    private static List<ValidationRule<LocalReplay>> ValidConfigForReplay(string? replayName)
    {
        return Valid.Rules<LocalReplay>()
            .Must(x => x.Details.Turns.Count > 0, $"Replay {replayName} has no turns, cannot create gif");
    }

    private static List<ValidationRule<List<LocalReplay>>> Only1ReplayFound(string? replayName) =>
        Valid.Rules<List<LocalReplay>>()
            .MustNot(x => x.Count == 0, $"No replays found with name: {replayName}")
            .MustNot(x => x.Count > 1, $"More than one replay found matching pattern: {replayName}");

    private static List<ValidationRule<Config>> ValidConfig() =>
        Valid.Rules<Config>()
            .Must(x => !string.IsNullOrWhiteSpace(x.ReplayName), "No replay provided for the Gif being created")
            .Must(x => x.Turn != default, "No turn provided for the Gif being created");

    private async Task<List<LocalReplay>> FindReplays(string pattern, CancellationToken cancellationToken) =>
    [
        .. await replayRetriever.Retrieve(pattern, cancellationToken)
    ];

    private sealed record Config(
        string? ReplayName,
        uint Turn,
        uint FramesPerSecond,
        uint Speed,
        uint StartOffset,
        uint EndOffset);
}

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
    public static readonly Option<string> ReplayName = new("--replay", "-r") { Description = "The replay name" };

    public static readonly Option<uint> Turn = new("--turn", "-t") { Description = "The turn number" };

    public static readonly Option<uint> FramesPerSecond = new("--frames-per-second", "-fps")
    {
        Description = "The number of frames per second",
        DefaultValueFactory = _ => 5
    };

    public static readonly Option<uint> Speed = new("--speed", "-s")
    {
        Description = "Speed multiplier for the gif",
        DefaultValueFactory = _ => 2,
    };

    public static readonly Option<uint> StartOffset = new("--start-offset", "-so")
    {
        Description = "Offset for the start of the gif in seconds",
        DefaultValueFactory = _ => 0
    };

    public static readonly Option<uint> EndOffset = new("--end-offset", "-eo")
    {
        Description = "Offset for the end of the gif in seconds",
        DefaultValueFactory = _ => 0,
    };

    public CreateGif()
        : base("gif", "Create animated gifs of replays (.gif files)")
    {
        Options.Add(ReplayName);
        Options.Add(Turn);
        Options.Add(FramesPerSecond);
        Options.Add(Speed);
        Options.Add(StartOffset);
        Options.Add(EndOffset);
    }
}

internal sealed class CreateGifHandler(
    IResourceCreator<LocalGif, LocalGifCreateParameters> gifCreator,
    IResourceRetriever<LocalReplay> replayRetriever,
    ILogger<CreateGifHandler> logger) : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Gif.SpanNameCreate);
        var replayName = parseResult.GetValue(CreateGif.ReplayName);
        var turn = parseResult.GetValue(CreateGif.Turn);
        var fps = parseResult.GetValue(CreateGif.FramesPerSecond);
        var speed = parseResult.GetValue(CreateGif.Speed);
        var startOffset = parseResult.GetValue(CreateGif.StartOffset);
        var endOffset = parseResult.GetValue(CreateGif.EndOffset);

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

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using Worms.Cli.Commands.Validation;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class DeleteReplay : Command
{
    public static readonly Argument<string> ReplayName = new("name", "The name of the Replay to be deleted");

    public DeleteReplay()
        : base("replay", "Delete replays (.WAgame files)")
    {
        AddAlias("replays");
        AddAlias("WAgame");
        AddArgument(ReplayName);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DeleteReplayHandler(
    IResourceRetriever<LocalReplay> resourceRetriever,
    IResourceDeleter<LocalReplay> resourceDeleter,
    ILogger<DeleteReplayHandler> logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        var name = context.ParseResult.GetValueForArgument(DeleteReplay.ReplayName);
        var cancellationToken = context.GetCancellationToken();

        var replay = await name.Validate(NameIsNotEmpty())
            .Map(FindReplays())
            .Validate(Only1ReplayFound())
            .Map(x => x.Single())
            .ConfigureAwait(false);

        if (!replay.IsValid)
        {
            replay.LogErrors(logger);
            return 1;
        }

        resourceDeleter.Delete(replay.Value);
        return 0;

        static IEnumerable<ValidationRule<string>> NameIsNotEmpty() =>
            new RulesFor<string>().Must(
                    x => !string.IsNullOrWhiteSpace(x),
                    "No name provided for the replay to be deleted.")
                .Build();

        Func<string, Task<IReadOnlyCollection<LocalReplay>>> FindReplays() =>
            async x => await resourceRetriever.Retrieve(x, cancellationToken).ConfigureAwait(false);

        IEnumerable<ValidationRule<IReadOnlyCollection<LocalReplay>>> Only1ReplayFound() =>
            new RulesFor<IReadOnlyCollection<LocalReplay>>()
                .MustNot(x => x.Count == 0, $"No replay found with name: {name}")
                .MustNot(x => x.Count > 1, $"More than one replay found with name matching: {name}")
                .Build();
    }
}

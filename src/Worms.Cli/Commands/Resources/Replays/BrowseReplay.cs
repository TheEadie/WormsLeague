using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Local.Folders;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class BrowseReplay : Command
{
    public BrowseReplay()
        : base("replay", "Open the folder containing the local replays")
    {
        AddAlias("replays");
        AddAlias("WAgame");
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BrowseReplayHandler(IWormsLocator wormsLocator, IFolderOpener folderOpener, ILogger logger)
    : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var worms = wormsLocator.Find();

        if (!worms.IsInstalled)
        {
            logger.Error("Worms is not installed");
            return Task.FromResult(1);
        }

        logger.Verbose($"Opening scheme folder: {worms.ReplayFolder}");
        folderOpener.OpenFolder(worms.ReplayFolder);
        return Task.FromResult(0);
    }
}

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
internal sealed class BrowseReplayHandler : ICommandHandler
{
    private readonly IWormsLocator _wormsLocator;
    private readonly IFolderOpener _folderOpener;
    private readonly ILogger _logger;

    public BrowseReplayHandler(IWormsLocator wormsLocator, IFolderOpener folderOpener, ILogger logger)
    {
        _wormsLocator = wormsLocator;
        _folderOpener = folderOpener;
        _logger = logger;
    }

    public int Invoke(InvocationContext context) => Task.Run(async () => await InvokeAsync(context)).Result;

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var worms = _wormsLocator.Find();

        if (!worms.IsInstalled)
        {
            _logger.Error("Worms is not installed");
            return Task.FromResult(1);
        }

        _logger.Verbose($"Opening scheme folder: {worms.ReplayFolder}");
        _folderOpener.OpenFolder(worms.ReplayFolder);
        return Task.FromResult(0);
    }
}

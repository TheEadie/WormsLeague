using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Local.Folders;

namespace Worms.Cli.Commands.Resources.Replays;

internal sealed class BrowseReplay : Command
{
    public BrowseReplay()
        : base("replay", "Open the folder containing the local replays")
    {
        Aliases.Add("replays");
        Aliases.Add("WAgame");
    }
}

internal sealed class BrowseReplayHandler(
    IWormsArmageddon wormsArmageddon,
    IFolderOpener folderOpener,
    ILogger<BrowseReplayHandler> logger) : AsynchronousCommandLineAction
{
    public override Task<int> InvokeAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Replay.SpanNameBrowse);
        var worms = wormsArmageddon.FindInstallation();

        if (!worms.IsInstalled)
        {
            logger.LogError("Worms is not installed");
            return Task.FromResult(1);
        }

        logger.LogDebug("Opening replay folder: {Folder}", worms.ReplayFolder);
        folderOpener.OpenFolder(worms.ReplayFolder);
        return Task.FromResult(0);
    }
}

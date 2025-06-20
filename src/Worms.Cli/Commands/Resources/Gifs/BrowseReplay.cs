using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Local.Folders;

namespace Worms.Cli.Commands.Resources.Gifs;

internal sealed class BrowseGif : Command
{
    public BrowseGif()
        : base("gif", "Open the folder containing the local gifs") =>
        Aliases.Add("gifs");
}

internal sealed class BrowseGifHandler(
    IWormsArmageddon wormsArmageddon,
    IFolderOpener folderOpener,
    ILogger<BrowseGifHandler> logger) : AsynchronousCommandLineAction
{
    public override Task<int> InvokeAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Gif.SpanNameBrowse);
        var worms = wormsArmageddon.FindInstallation();

        if (!worms.IsInstalled)
        {
            logger.LogError("Worms is not installed");
            return Task.FromResult(1);
        }

        logger.LogDebug("Opening capture folder: {Folder}", worms.CaptureFolder);
        folderOpener.OpenFolder(worms.CaptureFolder);
        return Task.FromResult(0);
    }
}

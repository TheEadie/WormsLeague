using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Local.Folders;

namespace Worms.Cli.Commands.Resources.Schemes;

internal sealed class BrowseScheme : Command
{
    public BrowseScheme()
        : base("scheme", "Open the folder containing the local schemes")
    {
        Aliases.Add("schemes");
        Aliases.Add("wsc");
    }
}

internal sealed class BrowseSchemeHandler(
    IWormsArmageddon wormsArmageddon,
    IFolderOpener folderOpener,
    ILogger<BrowseSchemeHandler> logger) : AsynchronousCommandLineAction
{
    public override Task<int> InvokeAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Scheme.SpanNameBrowse);
        var worms = wormsArmageddon.FindInstallation();

        if (!worms.IsInstalled)
        {
            logger.LogError("Worms is not installed");
            return Task.FromResult(1);
        }

        logger.LogDebug("Opening scheme folder: {Folder}", worms.SchemesFolder);
        folderOpener.OpenFolder(worms.SchemesFolder);
        return Task.FromResult(0);
    }
}

using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Local.Folders;

namespace Worms.Cli.Commands.Resources.Schemes;

internal sealed class BrowseScheme : Command
{
    public BrowseScheme()
        : base("scheme", "Open the folder containing the local schemes")
    {
        AddAlias("schemes");
        AddAlias("wsc");
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BrowseSchemeHandler(IWormsLocator wormsLocator, IFolderOpener folderOpener, ILogger logger)
    : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).Result;

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var worms = wormsLocator.Find();

        if (!worms.IsInstalled)
        {
            logger.Error("Worms is not installed");
            return Task.FromResult(1);
        }

        logger.Verbose($"Opening scheme folder: {worms.SchemesFolder}");
        folderOpener.OpenFolder(worms.SchemesFolder);
        return Task.FromResult(0);
    }
}

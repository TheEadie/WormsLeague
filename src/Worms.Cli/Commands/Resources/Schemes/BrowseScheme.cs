using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Local.Folders;

namespace Worms.Commands.Resources.Schemes;

internal class BrowseScheme : Command
{
    public BrowseScheme()
        : base("scheme", "Open the folder containing the local schemes")
    {
        AddAlias("schemes");
        AddAlias("wsc");
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal class BrowseSchemeHandler : ICommandHandler
{
    private readonly IWormsLocator _wormsLocator;
    private readonly IFolderOpener _folderOpener;
    private readonly ILogger _logger;

    public BrowseSchemeHandler(IWormsLocator wormsLocator, IFolderOpener folderOpener, ILogger logger)
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

        _logger.Verbose($"Opening scheme folder: {worms.SchemesFolder}");
        _folderOpener.OpenFolder(worms.SchemesFolder);
        return Task.FromResult(0);
    }
}

using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Cli.Resources.Local.Folders;

namespace Worms.Cli.Commands.Resources.Gifs;

internal sealed class BrowseGif : Command
{
    public BrowseGif()
        : base("gif", "Open the folder containing the local gifs") =>
        AddAlias("gifs");
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class BrowseGifHandler(IWormsLocator wormsLocator, IFolderOpener folderOpener, ILogger logger)
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

        logger.Verbose($"Opening scheme folder: {worms.CaptureFolder}");
        folderOpener.OpenFolder(worms.CaptureFolder);
        return Task.FromResult(0);
    }
}

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Worms.Cli.Commands;
using Worms.Cli.Commands.Resources;
using Worms.Cli.Commands.Resources.Games;
using Worms.Cli.Commands.Resources.Gifs;
using Worms.Cli.Commands.Resources.Replays;
using Worms.Cli.Commands.Resources.Schemes;
using Version = Worms.Cli.Commands.Version;

namespace Worms.Cli;

internal static class CliStructure
{
    internal static CommandLineBuilder BuildCommandLine(IServiceProvider serviceProvider)
    {
        var rootCommand = new Root();
        rootCommand.Add<Auth, AuthHandler>(serviceProvider);
        rootCommand.Add<Version, VersionHandler>(serviceProvider);
        rootCommand.Add<Update, UpdateHandler>(serviceProvider);
        rootCommand.Add<Setup, SetupHandler>(serviceProvider);
        rootCommand.Add<Host, HostHandler>(serviceProvider);

        var viewCommand = new View();
        viewCommand.Add<ViewReplay, ViewReplayHandler>(serviceProvider);
        rootCommand.AddCommand(viewCommand);

        var processCommand = new Process();
        processCommand.Add<ProcessReplay, ProcessReplayHandler>(serviceProvider);
        rootCommand.AddCommand(processCommand);

        var getCommand = new Get();
        getCommand.Add<GetScheme, GetSchemeHandler>(serviceProvider);
        getCommand.Add<GetReplay, GetReplayHandler>(serviceProvider);
        getCommand.Add<GetGame, GetGameHandler>(serviceProvider);
        rootCommand.AddCommand(getCommand);

        var deleteCommand = new Delete();
        deleteCommand.Add<DeleteScheme, DeleteSchemeHandler>(serviceProvider);
        deleteCommand.Add<DeleteReplay, DeleteReplayHandler>(serviceProvider);
        rootCommand.AddCommand(deleteCommand);

        var createCommand = new Create();
        createCommand.Add<CreateScheme, CreateSchemeHandler>(serviceProvider);
        createCommand.Add<CreateGif, CreateGifHandler>(serviceProvider);
        rootCommand.AddCommand(createCommand);

        var browseCommand = new Browse();
        browseCommand.Add<BrowseScheme, BrowseSchemeHandler>(serviceProvider);
        browseCommand.Add<BrowseReplay, BrowseReplayHandler>(serviceProvider);
        browseCommand.Add<BrowseGif, BrowseGifHandler>(serviceProvider);
        rootCommand.AddCommand(browseCommand);

        return new CommandLineBuilder(rootCommand);
    }

    private static void Add<TC, TH>(this Command rootCommand, IServiceProvider serviceProvider)
        where TC : Command, new() where TH : ICommandHandler
    {
        var auth = new TC();
        auth.SetHandler(context => serviceProvider.GetRequiredService<TH>().InvokeAsync(context));
        rootCommand.AddCommand(auth);
    }
}

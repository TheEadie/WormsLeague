using System.CommandLine.Builder;
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
    internal static CommandLineBuilder BuildCommandLine()
    {
        var rootCommand = new Root();
        rootCommand.AddCommand(new Auth());
        rootCommand.AddCommand(new Version());
        rootCommand.AddCommand(new Update());
        rootCommand.AddCommand(new Setup());
        rootCommand.AddCommand(new Host());

        var viewCommand = new View();
        viewCommand.AddCommand(new ViewReplay());
        rootCommand.AddCommand(viewCommand);

        var processCommand = new Process();
        processCommand.AddCommand(new ProcessReplay());
        rootCommand.AddCommand(processCommand);

        var getCommand = new Get();
        getCommand.AddCommand(new GetScheme());
        getCommand.AddCommand(new GetReplay());
        getCommand.AddCommand(new GetGame());
        rootCommand.AddCommand(getCommand);

        var deleteCommand = new Delete();
        deleteCommand.AddCommand(new DeleteScheme());
        deleteCommand.AddCommand(new DeleteReplay());
        rootCommand.AddCommand(deleteCommand);

        var createCommand = new Create();
        createCommand.AddCommand(new CreateScheme());
        createCommand.AddCommand(new CreateGif());
        rootCommand.AddCommand(createCommand);

        var browseCommand = new Browse();
        browseCommand.AddCommand(new BrowseScheme());
        browseCommand.AddCommand(new BrowseReplay());
        browseCommand.AddCommand(new BrowseGif());
        rootCommand.AddCommand(browseCommand);

        return new CommandLineBuilder(rootCommand);
    }
}

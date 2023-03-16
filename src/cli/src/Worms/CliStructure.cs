﻿using System.CommandLine.Builder;
using Worms.Commands;
using Worms.Commands.Resources;
using Worms.Commands.Resources.Games;
using Worms.Commands.Resources.Gifs;
using Worms.Commands.Resources.Replays;
using Worms.Commands.Resources.Schemes;

namespace Worms;

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

        return new CommandLineBuilder(rootCommand);
    }
}
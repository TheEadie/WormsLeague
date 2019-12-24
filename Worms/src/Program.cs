﻿using System;
using McMaster.Extensions.CommandLineUtils;
using Worms.Commands;

namespace Worms
{
    class Program
    {
        public static int Main(string[] args)
        {
            var console = new PhysicalConsole();
            var app = new CommandLineApplication<Root>(console, Environment.CurrentDirectory, true);
            app.Conventions.UseDefaultConventions();
            ConfigureHelp(app);

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                var reporter = new ConsoleReporter(console);
                reporter.Error($"{ex.Message}{Environment.NewLine}");
                ex.Command.ShowHelp();
                return 1;
            }
        }

        private static void ConfigureHelp(CommandLineApplication cmd)
        {
            cmd.UsePagerForHelpText = false;

            foreach (var subcmd in cmd.Commands)
            {
                ConfigureHelp(subcmd);
            }
        }
    }
}

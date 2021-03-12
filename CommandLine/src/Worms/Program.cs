﻿using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using McMaster.Extensions.CommandLineUtils;
using Worms.Commands;
using Worms.Container;

namespace Worms
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var console = PhysicalConsole.Singleton;
            var app = new CommandLineApplication<Root>(console, Environment.CurrentDirectory);
            app.Conventions.UseDefaultConventions().UseConstructorInjection(SetUpDi());
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

            foreach (var subCmd in cmd.Commands)
            {
                ConfigureHelp(subCmd);
            }
        }

        private static IServiceProvider SetUpDi()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<CliModule>();

            var container = builder.Build();

            return new AutofacServiceProvider(container);
        }
    }
}

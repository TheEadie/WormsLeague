using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Newtonsoft.Json;
using WormsRandomizer.Config;
using WormsRandomizer.Flags;
using WormsRandomizer.WormsScheme;

namespace WormsRandomizer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = CreateContainer();
            var app = container.Resolve<IRandomizerApp>();

            if (args.Length != 0)
            {
                RunRandomizer(app, args);
                return;
            }

            app.PrintHelp();
            do
            {
                Console.WriteLine("----------------");
                Console.WriteLine("Enter arguments:");
                args = Console.ReadLine()?.Split(' ') ?? new string[0];
                RunRandomizer(app, args);
            } while (args.Length != 0);
        }

        private static void RunRandomizer(IRandomizerApp app, string[] args)
        {
            if (args.Length == 1 && args[0] == "-weapons")
            {
                app.PrintWeaponList();
            }
            else if (args.Length == 1 && args[0] == "-h")
            {
                app.PrintHelp();
            }
            else if(args.Length >= 1)
            {
                app.DoRandomizer(args);
            }
        }

        private static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<MainModule>();
            return containerBuilder.Build();
        }
    }
}

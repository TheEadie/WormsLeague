using System.Collections.Generic;
using System.IO.Abstractions;
using Autofac;
using Worms.Components;
using Worms.Components.Updaters;
using Worms.GameRunner;
using Worms.Updates;
using Worms.Updates.Installers;
using Worms.Updates.Repositories;

namespace Worms
{
    public class CliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // GameRunner
            builder.RegisterType<SteamService>().As<ISteamService>();
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();

            // Components
            builder.RegisterType<OutsideOfToolUpdater>();
            builder.RegisterType<GitHubReleaseUpdater>();

            // Updates
            builder.RegisterType<FileCopierInstaller>().As<IFileCopierInstaller>();
            builder.RegisterType<GitHubReleaseRepository>();
            builder.RegisterType<ComponentFactory>();
            builder.RegisterType<ComponentOperations>();

            builder.Register(c => 
                {
                    var factory = c.Resolve<ComponentFactory>();
                    var components = new List<Component> { factory.CreateCli(), factory.CreateGame() };
                    return components;
                }
            ).As<IEnumerable<Component>>();
        }
    }
}

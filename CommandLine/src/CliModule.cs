using System.IO.Abstractions;
using Autofac;
using Worms.Components;
using Worms.Components.Repositories;
using Worms.Components.Updates;
using Worms.GameRunner;

namespace Worms
{
    public class CliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // GameRunner
            builder.RegisterType<SteamService>().As<ISteamService>();
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();

            // Components
            builder.RegisterType<Cli>().As<IComponent>();
            builder.RegisterType<Game>().As<IComponent>();

            // Updates
            //builder.Register(c => new FolderRepository(@"D:\WormsRepo", c.Resolve<IFileSystem>())).As<IUpdateRepository>();

            var repo = new GitHubReleaseRepository();
            repo.Connect("TheEadie", "WormsLeague", "giftool/v");

            builder.RegisterInstance(repo).As<IUpdateRepository>();
            builder.RegisterType<ComponentUpdater>().As<IComponentUpdater>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
        }
    }
}

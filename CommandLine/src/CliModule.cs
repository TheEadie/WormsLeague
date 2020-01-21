using System.IO.Abstractions;
using Autofac;
using Worms.Components;
using Worms.Components.Updaters;
using Worms.Components.Updaters.GitHubReleaseUpdater;
using Worms.Components.Updaters.OutsideOfToolUpdater;
using worms.Configuration;
using worms.Configuration.SecureStorage;
using Worms.GameRunner;
using Worms.Updates.PackageManagers;

namespace Worms
{
    public class CliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Config
            builder.RegisterType<WindowsCredentialStorage>().As<ICredentialStorage>();
            builder.RegisterType<ConfigManager>();

            // GameRunner
            builder.RegisterType<SteamService>().As<ISteamService>();
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();

            // Components
            builder.RegisterType<ComponentsRepository>();
            builder.RegisterType<ComponentOperations>();
            builder.RegisterType<OutsideOfToolUpdater>().As<IUpdater<OutsideOfToolUpdateConfig>>();
            builder.RegisterType<GitHubReleaseUpdater>().As<IUpdater<GitHubReleaseUpdateConfig>>();

            // Updates
            builder.RegisterType<GitHubReleaseRepository>();
        }
    }
}

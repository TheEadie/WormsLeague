using System.IO.Abstractions;
using Autofac;
using worms.Configuration;
using worms.Configuration.SecureStorage;
using worms.GameRunner.Windows;
using Worms.Cli;
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

            // CLI
            builder.RegisterType<GitHubReleasePackageManager>();
            builder.RegisterType<CliUpdater>();
            builder.RegisterType<CliInfoRetriever>();
        }
    }
}

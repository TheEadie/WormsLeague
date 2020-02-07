using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Autofac;
using Worms.Cli;
using Worms.Configuration;
using Worms.Slack;
using Worms.Updates.PackageManagers;

namespace Worms.Container
{
    public class CliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterOSModules(builder);

            // FileSystem
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Config
            builder.RegisterType<ConfigManager>().As<IConfigManager>();

            // CLI
            builder.RegisterType<GitHubReleasePackageManager>();
            builder.RegisterType<CliUpdater>();
            builder.RegisterType<CliInfoRetriever>();

            // Annoucer
            builder.RegisterType<SlackAnnouncer>().As<ISlackAnnouncer>();
        }

        private static void RegisterOSModules(ContainerBuilder builder)
        {
            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.RegisterModule<WindowsCliModule>();
            }

            // Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                builder.RegisterModule<LinuxCliModule>();
            }
        }
    }
}

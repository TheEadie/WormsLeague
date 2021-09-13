using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Autofac;
using Worms.Armageddon.Game.Modules;
using Worms.Armageddon.Resources.Modules;
using Worms.Cli;
using Worms.Cli.PackageManagers;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Cli.Resources.Modules;
using Worms.Configuration;
using Worms.League;
using Worms.Resources;
using Worms.Resources.Replays;
using Worms.Resources.Schemes;
using Worms.Slack;

namespace Worms.Modules
{
    public class CliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterOsModules(builder);

            // FileSystem
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Config
            builder.RegisterType<ConfigManager>().As<IConfigManager>();

            // CLI
            builder.RegisterType<GitHubReleasePackageManager>();
            builder.RegisterType<CliUpdater>();
            builder.RegisterType<CliInfoRetriever>();

            // Announcer
            builder.RegisterType<SlackAnnouncer>().As<ISlackAnnouncer>();

            // League
            builder.RegisterType<LeagueUpdater>();

            // Schemes
            builder.RegisterType<SchemeTextPrinter>().As<IResourcePrinter<LocalScheme>>();

            // Replays
            builder.RegisterType<ReplayTextPrinter>().As<IResourcePrinter<LocalReplay>>();

            builder.RegisterGeneric(typeof(ResourceGetter<>)).As(typeof(ResourceGetter<>));
            builder.RegisterGeneric(typeof(ResourceDeleter<>)).As(typeof(ResourceDeleter<>));
            builder.RegisterGeneric(typeof(ResourceViewer<,>)).As(typeof(ResourceViewer<,>));

            builder.RegisterModule<ArmageddonGameModule>();
            builder.RegisterModule<ArmageddonResourcesModule>();
            builder.RegisterModule<CliResourcesModule>();

        }

        private static void RegisterOsModules(ContainerBuilder builder)
        {
            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.RegisterModule<WindowsModule>();
            }

            // Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                builder.RegisterModule<LinuxModule>();
            }
        }
    }
}

using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Autofac;
using Worms.Armageddon.Files.Modules;
using Worms.Armageddon.Game.Modules;
using Worms.Cli.CommandLine;
using Worms.Cli.CommandLine.PackageManagers;
using Worms.Cli.Configuration;
using Worms.Cli.League;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Games;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Cli.Resources.Modules;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Replays;
using Worms.Cli.Resources.Schemes;
using Worms.Cli.Slack;

namespace Worms.Cli.Modules
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

            // Auth
            builder.RegisterType<TokenStore>().As<ITokenStore>();
            builder.RegisterType<DeviceCodeLoginService>().As<ILoginService>();

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

            // Games
            builder.RegisterType<GameTextPrinter>().As<IResourcePrinter<RemoteGame>>();

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

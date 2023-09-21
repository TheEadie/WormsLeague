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
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Replays;
using Worms.Cli.Resources.Schemes;
using Worms.Cli.Slack;

namespace Worms.Cli.Modules;

public class CliModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterOsModules(builder);

        // FileSystem
        _ = builder.RegisterType<FileSystem>().As<IFileSystem>();

        // Config
        _ = builder.RegisterType<ConfigManager>().As<IConfigManager>();

        // CLI
        _ = builder.RegisterType<GitHubReleasePackageManager>();
        _ = builder.RegisterType<CliUpdater>();
        _ = builder.RegisterType<CliInfoRetriever>();

        // Announcer
        _ = builder.RegisterType<SlackAnnouncer>().As<ISlackAnnouncer>();

        // League
        _ = builder.RegisterType<LeagueUpdater>();

        // Schemes
        _ = builder.RegisterType<SchemeTextPrinter>().As<IResourcePrinter<LocalScheme>>();

        // Replays
        _ = builder.RegisterType<ReplayTextPrinter>().As<IResourcePrinter<LocalReplay>>();

        // Games
        _ = builder.RegisterType<GameTextPrinter>().As<IResourcePrinter<RemoteGame>>();

        _ = builder.RegisterGeneric(typeof(ResourceGetter<>)).As(typeof(ResourceGetter<>));
        _ = builder.RegisterGeneric(typeof(ResourceDeleter<>)).As(typeof(ResourceDeleter<>));
        _ = builder.RegisterGeneric(typeof(ResourceViewer<,>)).As(typeof(ResourceViewer<,>));

        _ = builder.RegisterModule<ArmageddonGameModule>();
        _ = builder.RegisterModule<ArmageddonResourcesModule>();
        _ = builder.RegisterModule<CliResourcesModule>();

    }

    private static void RegisterOsModules(ContainerBuilder builder)
    {
        // Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _ = builder.RegisterModule<WindowsModule>();
        }

        // Linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _ = builder.RegisterModule<LinuxModule>();
        }
    }
}

using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Autofac;
using Worms.Armageddon.Files.Schemes.Random;
using Worms.Cli.Resources.Local.Folders;
using Worms.Cli.Resources.Local.Gifs;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Cli.Resources.Remote;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Remote.Replays;
using Worms.Cli.Resources.Remote.Schemes;
using Worms.Cli.Resources.Remote.Updates;

namespace Worms.Cli.Resources.Modules;

public class CliResourcesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // OS Specific
        RegisterOsModules(builder);

        // FileSystem
        _ = builder.RegisterType<FileSystem>().As<IFileSystem>();

        // Random Configurations
        _ = builder.RegisterType<RandomSchemeGenerator>().As<IRandomSchemeGenerator>();

        // Schemes
        _ = builder.RegisterType<LocalSchemesRetriever>().As<IResourceRetriever<LocalScheme>>();
        _ = builder.RegisterType<LocalSchemeCreator>().As<IResourceCreator<LocalScheme, LocalSchemeCreateParameters>>();
        _ = builder.RegisterType<LocalSchemeRandomCreator>()
            .As<IResourceCreator<LocalScheme, LocalSchemeCreateRandomParameters>>();
        _ = builder.RegisterType<LocalSchemeDeleter>().As<IResourceDeleter<LocalScheme>>();
        _ = builder.RegisterType<RemoteSchemeRetriever>().As<IRemoteSchemeRetriever>();
        _ = builder.RegisterType<RemoteSchemeDownloader>().As<IRemoteSchemeDownloader>();

        // Replays
        _ = builder.RegisterType<LocalReplayLocator>().As<ILocalReplayLocator>();
        _ = builder.RegisterType<LocalReplayRetriever>().As<IResourceRetriever<LocalReplay>>();
        _ = builder.RegisterType<LocalReplayDeleter>().As<IResourceDeleter<LocalReplay>>();
        _ = builder.RegisterType<LocalReplayViewer>().As<IResourceViewer<LocalReplay, LocalReplayViewParameters>>();
        _ = builder.RegisterType<RemoteReplayCreator>()
            .As<IResourceCreator<RemoteReplay, RemoteReplayCreateParameters>>();

        // Gifs
        _ = builder.RegisterType<LocalGifCreator>().As<IResourceCreator<LocalGif, LocalGifCreateParameters>>();

        // API
        _ = builder.RegisterType<TokenStore>().As<ITokenStore>();
        _ = builder.RegisterType<DeviceCodeLoginService>().As<ILoginService>();
        _ = builder.RegisterType<AccessTokenRefreshService>().As<IAccessTokenRefreshService>();
        _ = builder.RegisterType<DeviceCodeLoginService>().As<ILoginService>();
        _ = builder.RegisterType<WormsServerApi>().As<IWormsServerApi>();

        // Games
        _ = builder.RegisterType<RemoteGameRetriever>().As<IResourceRetriever<RemoteGame>>();
        _ = builder.RegisterType<RemoteGameCreator>().As<IResourceCreator<RemoteGame, string>>();
        _ = builder.RegisterType<RemoteGameUpdater>().As<IRemoteGameUpdater>();

        // CLI Updates
        _ = builder.RegisterType<CliUpdateRetriever>().As<ICliUpdateRetriever>();
    }

    private static void RegisterOsModules(ContainerBuilder builder)
    {
        // Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _ = builder.RegisterType<WindowsFolderOpener>().As<IFolderOpener>();
            _ = builder.RegisterType<WindowsCliUpdateDownloader>().As<ICliUpdateDownloader>();
        }

        // Linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _ = builder.RegisterType<LinuxFolderOpener>().As<IFolderOpener>();
            _ = builder.RegisterType<LinuxCliUpdateDownloader>().As<ICliUpdateDownloader>();
        }
    }
}

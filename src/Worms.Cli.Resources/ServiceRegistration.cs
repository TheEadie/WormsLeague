using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Files.Schemes.Random;
using Worms.Cli.Resources.Local.Gifs;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Cli.Resources.Remote;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Remote.Leagues;
using Worms.Cli.Resources.Remote.Replays;
using Worms.Cli.Resources.Remote.Schemes;
using Worms.Cli.Resources.Remote.Updates;

namespace Worms.Cli.Resources;

public static class ServiceRegistration
{
    public static IServiceCollection AddWormsCliResourcesServices(this IServiceCollection builder) =>
        builder.AddOsServices()
            .AddTransient<IFileSystem, FileSystem>()
            .AddTransient<IRandomSchemeGenerator, RandomSchemeGenerator>()
            .AddTransient<IResourceRetriever<LocalScheme>, LocalSchemesRetriever>()
            .AddTransient<IResourceCreator<LocalScheme, LocalSchemeCreateParameters>, LocalSchemeCreator>()
            .AddTransient<IResourceCreator<LocalScheme, LocalSchemeCreateRandomParameters>, LocalSchemeRandomCreator>()
            .AddTransient<IResourceDeleter<LocalScheme>, LocalSchemeDeleter>()
            .AddTransient<IRemoteLeagueRetriever, RemoteLeagueRetriever>()
            .AddTransient<IRemoteSchemeDownloader, RemoteSchemeDownloader>()
            .AddTransient<ILocalReplayLocator, LocalReplayLocator>()
            .AddTransient<IResourceRetriever<LocalReplay>, LocalReplayRetriever>()
            .AddTransient<IResourceDeleter<LocalReplay>, LocalReplayDeleter>()
            .AddTransient<IResourceViewer<LocalReplay, LocalReplayViewParameters>, LocalReplayViewer>()
            .AddTransient<IResourceCreator<RemoteReplay, RemoteReplayCreateParameters>, RemoteReplayCreator>()
            .AddTransient<IResourceCreator<LocalGif, LocalGifCreateParameters>, LocalGifCreator>()
            .AddTransient<ITokenStore, TokenStore>()
            .AddTransient<ILoginService, DeviceCodeLoginService>()
            .AddTransient<IAccessTokenRefreshService, AccessTokenRefreshService>()
            .AddTransient<IWormsServerApi, WormsServerApi>()
            .AddTransient<IResourceRetriever<RemoteGame>, RemoteGameRetriever>()
            .AddTransient<IResourceCreator<RemoteGame, string>, RemoteGameCreator>()
            .AddTransient<IRemoteGameUpdater, RemoteGameUpdater>()
            .AddTransient<ICliUpdateRetriever, CliUpdateRetriever>();

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
    private static IServiceCollection AddOsServices(this IServiceCollection builder)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return builder.AddWindowsServices();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return builder.AddLinuxServices();
        }

        throw new PlatformNotSupportedException("This platform is not supported");
    }
}

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Files.Schemes.Random;
using Worms.Cli.Resources.Local.Folders;
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
            .AddScoped<IFileSystem, FileSystem>()
            .AddScoped<IRandomSchemeGenerator, RandomSchemeGenerator>()
            .AddScoped<IResourceRetriever<LocalScheme>, LocalSchemesRetriever>()
            .AddScoped<IResourceCreator<LocalScheme, LocalSchemeCreateParameters>, LocalSchemeCreator>()
            .AddScoped<IResourceDeleter<LocalScheme>, LocalSchemeDeleter>()
            .AddScoped<IRemoteLeagueRetriever, RemoteLeagueRetriever>()
            .AddScoped<IRemoteSchemeDownloader, RemoteSchemeDownloader>()
            .AddScoped<ILocalReplayLocator, LocalReplayLocator>()
            .AddScoped<IResourceRetriever<LocalReplay>, LocalReplayRetriever>()
            .AddScoped<IResourceDeleter<LocalReplay>, LocalReplayDeleter>()
            .AddScoped<IResourceViewer<LocalReplay, LocalReplayViewParameters>, LocalReplayViewer>()
            .AddScoped<IResourceCreator<RemoteReplay, RemoteReplayCreateParameters>, RemoteReplayCreator>()
            .AddScoped<IResourceCreator<LocalGif, LocalGifCreateParameters>, LocalGifCreator>()
            .AddScoped<ITokenStore, TokenStore>()
            .AddScoped<ILoginService, DeviceCodeLoginService>()
            .AddScoped<IAccessTokenRefreshService, AccessTokenRefreshService>()
            .AddScoped<IUserDetailsService, UserDetailsService>()
            .AddScoped<IWormsServerApi, WormsServerApi>()
            .AddScoped<IResourceRetriever<RemoteGame>, RemoteGameRetriever>()
            .AddScoped<IResourceCreator<RemoteGame, string>, RemoteGameCreator>()
            .AddScoped<IRemoteGameUpdater, RemoteGameUpdater>()
            .AddScoped<ICliUpdateRetriever, CliUpdateRetriever>();

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

    private static IServiceCollection AddLinuxServices(this IServiceCollection builder) =>
        builder.AddScoped<IFolderOpener, LinuxFolderOpener>()
            .AddScoped<ICliUpdateDownloader, LinuxCliUpdateDownloader>();

    private static IServiceCollection AddWindowsServices(this IServiceCollection builder) =>
        builder.AddScoped<IFolderOpener, WindowsFolderOpener>()
            .AddScoped<ICliUpdateDownloader, WindowsCliUpdateDownloader>();
}

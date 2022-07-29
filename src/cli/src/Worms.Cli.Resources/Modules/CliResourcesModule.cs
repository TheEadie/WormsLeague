using System.IO.Abstractions;
using Autofac;
using Worms.Armageddon.Resources.Schemes.Random;
using Worms.Cli.Resources.Local.Gifs;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Cli.Resources.Remote;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Resources.Remote.Games;

namespace Worms.Cli.Resources.Modules
{
    public class CliResourcesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // FileSystem
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Random Configurations
            builder.RegisterType<RandomSchemeGenerator>().As<IRandomSchemeGenerator>();

            // Schemes
            builder.RegisterType<LocalSchemesRetriever>().As<IResourceRetriever<LocalScheme>>();
            builder.RegisterType<LocalSchemeCreator>().As<IResourceCreator<LocalScheme, LocalSchemeCreateParameters>>();
            builder.RegisterType<LocalSchemeRandomCreator>().As<IResourceCreator<LocalScheme, LocalSchemeCreateRandomParameters>>();
            builder.RegisterType<LocalSchemeDeleter>().As<IResourceDeleter<LocalScheme>>();

            // Replays
            builder.RegisterType<LocalReplayLocator>().As<ILocalReplayLocator>();
            builder.RegisterType<LocalReplayRetriever>().As<IResourceRetriever<LocalReplay>>();
            builder.RegisterType<LocalReplayDeleter>().As<IResourceDeleter<LocalReplay>>();
            builder.RegisterType<LocalReplayViewer>().As<IResourceViewer<LocalReplay, LocalReplayViewParameters>>();

            // Gifs
            builder.RegisterType<LocalGifCreator>().As<IResourceCreator<LocalGif, LocalGifCreateParameters>>();
            
            // API
            builder.RegisterType<AccessTokenRefreshService>().As<IAccessTokenRefreshService>();
            builder.RegisterType<DeviceCodeLoginService>().As<ILoginService>();
            builder.RegisterType<WormsServerApi>().As<IWormsServerApi>();
            
            // Games
            builder.RegisterType<RemoteGameRetriever>().As<IResourceRetriever<RemoteGame>>();

        }
    }
}

using System.IO.Abstractions;
using Autofac;
using Worms.Cli.Resources.Local.Gifs;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Local.Schemes;

namespace Worms.Cli.Resources.Modules
{
    public class CliResourcesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // FileSystem
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Schemes
            builder.RegisterType<LocalSchemesRetriever>().As<IResourceRetriever<LocalScheme>>();
            builder.RegisterType<LocalSchemeCreator>().As<IResourceCreator<LocalSchemeCreateParameters>>();
            builder.RegisterType<LocalSchemeDeleter>().As<IResourceDeleter<LocalScheme>>();

            // Replays
            builder.RegisterType<LocalReplayLocator>().As<ILocalReplayLocator>();
            builder.RegisterType<LocalReplayRetriever>().As<IResourceRetriever<LocalReplay>>();
            builder.RegisterType<LocalReplayDeleter>().As<IResourceDeleter<LocalReplay>>();
            builder.RegisterType<LocalReplayViewer>().As<IResourceViewer<LocalReplay, LocalReplayViewParameters>>();

            // Gifs
            builder.RegisterType<LocalGifCreator>().As<IResourceCreator<LocalGifCreateParameters>>();
        }
    }
}

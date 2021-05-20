using System.IO.Abstractions;
using Autofac;
using Worms.Armageddon.Resources.Schemes;
using Worms.Cli.Resources.Replays;
using Worms.Cli.Resources.Schemes;

namespace Worms.Cli.Resources.Modules
{
    public class CliResourcesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // FileSystem
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Schemes
            builder.RegisterType<LocalSchemesRetriever>().As<IResourceRetriever<SchemeResource>>();
            builder.RegisterType<LocalSchemeDeleter>().As<IResourceDeleter<SchemeResource>>();

            // Replays
            builder.RegisterType<ReplayLocator>().As<IReplayLocator>();
            builder.RegisterType<LocalReplayRetriever>().As<IResourceRetriever<LocalReplay>>();
            builder.RegisterType<LocalReplayDeleter>().As<IResourceDeleter<LocalReplay>>();
        }
    }
}

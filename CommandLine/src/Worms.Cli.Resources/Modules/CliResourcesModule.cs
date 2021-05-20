using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Autofac;
using Worms.Armageddon.Game.Modules;
using Worms.Armageddon.Resources.Modules;
using Worms.Armageddon.Resources.Replays;
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
            builder.RegisterType<LocalReplayRetriever>().As<IResourceRetriever<ReplayResource>>();
            builder.RegisterType<LocalReplayDeleter>().As<IResourceDeleter<ReplayResource>>();
        }
    }
}

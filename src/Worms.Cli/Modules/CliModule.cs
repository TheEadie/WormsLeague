using System.IO.Abstractions;
using Autofac;
using Worms.Armageddon.Files.Modules;
using Worms.Armageddon.Game.Modules;
using Worms.Cli.CommandLine;
using Worms.Cli.Commands;
using Worms.Cli.Commands.Resources.Games;
using Worms.Cli.Commands.Resources.Gifs;
using Worms.Cli.Commands.Resources.Replays;
using Worms.Cli.Commands.Resources.Schemes;
using Worms.Cli.League;
using Worms.Cli.Resources;
using Worms.Cli.Resources.Games;
using Worms.Cli.Resources.Local.Replays;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Cli.Resources.Modules;
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Replays;
using Worms.Cli.Resources.Schemes;

namespace Worms.Cli.Modules;

public class CliModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Commands
        _ = builder.RegisterType<AuthHandler>();
        _ = builder.RegisterType<HostHandler>();
        _ = builder.RegisterType<UpdateHandler>();
        _ = builder.RegisterType<VersionHandler>();

        _ = builder.RegisterType<GetGameHandler>();

        _ = builder.RegisterType<BrowseGifHandler>();
        _ = builder.RegisterType<CreateGifHandler>();

        _ = builder.RegisterType<BrowseReplayHandler>();
        _ = builder.RegisterType<DeleteReplayHandler>();
        _ = builder.RegisterType<GetReplayHandler>();
        _ = builder.RegisterType<ProcessReplayHandler>();
        _ = builder.RegisterType<ViewReplayHandler>();

        _ = builder.RegisterType<BrowseSchemeHandler>();
        _ = builder.RegisterType<CreateSchemeHandler>();
        _ = builder.RegisterType<DeleteSchemeHandler>();
        _ = builder.RegisterType<GetSchemeHandler>();

        // FileSystem
        _ = builder.RegisterType<FileSystem>().As<IFileSystem>();

        // CLI
        _ = builder.RegisterType<CliUpdater>();
        _ = builder.RegisterType<CliInfoRetriever>();

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
}

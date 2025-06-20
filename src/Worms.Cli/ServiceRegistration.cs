using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
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
using Worms.Cli.Resources.Remote.Games;
using Worms.Cli.Resources.Replays;
using Worms.Cli.Resources.Schemes;

namespace Worms.Cli;

internal static class ServiceRegistration
{
    public static IServiceCollection AddWormsCliServices(this IServiceCollection builder) =>
        builder.AddScoped<AuthHandler>()
            .AddScoped<HostHandler>()
            .AddScoped<UpdateHandler>()
            .AddScoped<VersionHandler>()
            .AddScoped<GetGameHandler>()
            .AddScoped<BrowseGifHandler>()
            .AddScoped<CreateGifHandler>()
            .AddScoped<BrowseReplayHandler>()
            .AddScoped<DeleteReplayHandler>()
            .AddScoped<GetReplayHandler>()
            .AddScoped<ProcessReplayHandler>()
            .AddScoped<ViewReplayHandler>()
            .AddScoped<BrowseSchemeHandler>()
            .AddScoped<CreateSchemeHandler>()
            .AddScoped<DeleteSchemeHandler>()
            .AddScoped<GetSchemeHandler>()
            .AddScoped<IFileSystem, FileSystem>()
            .AddScoped<CliUpdater>()
            .AddScoped<CliInfoRetriever>()
            .AddScoped<LeagueUpdater>()
            .AddScoped<IResourcePrinter<LocalScheme>, SchemeTextPrinter>()
            .AddScoped<IResourcePrinter<LocalReplay>, ReplayTextPrinter>()
            .AddScoped<IResourcePrinter<RemoteGame>, GameTextPrinter>()
            .AddScoped(typeof(ResourceGetter<>), typeof(ResourceGetter<>))
            .AddScoped(typeof(ResourceDeleter<>), typeof(ResourceDeleter<>))
            .AddScoped(typeof(ResourceViewer<,>), typeof(ResourceViewer<,>))
            .AddScoped<Runner>();
}

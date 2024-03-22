using Microsoft.Extensions.DependencyInjection;
using Worms.Cli.Resources.Local.Folders;
using Worms.Cli.Resources.Remote.Updates;

namespace Worms.Cli.Resources;

internal static class ServiceRegistrationLinux
{
    public static IServiceCollection AddLinuxServices(this IServiceCollection builder) =>
        builder.AddTransient<IFolderOpener, LinuxFolderOpener>()
            .AddTransient<ICliUpdateDownloader, LinuxCliUpdateDownloader>();
}

using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Worms.Cli.Resources.Local.Folders;
using Worms.Cli.Resources.Remote.Updates;

namespace Worms.Cli.Resources;

[SupportedOSPlatform("windows")]
internal static class ServiceRegistrationWindows
{
    public static IServiceCollection AddWindowsServices(this IServiceCollection builder) =>
        builder.AddScoped<IFolderOpener, WindowsFolderOpener>()
            .AddScoped<ICliUpdateDownloader, WindowsCliUpdateDownloader>();
}

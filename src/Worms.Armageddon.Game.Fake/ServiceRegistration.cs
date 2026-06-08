using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Worms.Armageddon.Game.Fake;

public static class ServiceRegistration
{
    public static IServiceCollection AddFakeInstalledWormsArmageddonServices(
        this IServiceCollection builder,
        IFileSystem fileSystem,
        string? gamePath = null,
        Version? version = null,
        bool hostCreatesReplay = true)
    {
        var installed = new Installed(fileSystem, gamePath, version, hostCreatesReplay);
        return builder
            .AddScoped<IFileSystem>(_ => fileSystem)
            .AddScoped<IWormsArmageddon>(_ => installed)
            .AddScoped<WormsArmageddonFakeSetup>();
    }

    public static IServiceCollection AddFakeNotInstalledWormsArmageddonServices(this IServiceCollection builder) =>
        builder.AddScoped<IWormsArmageddon, NotInstalled>();
}

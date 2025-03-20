using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Worms.Armageddon.Game.Fake;

public static class ServiceRegistration
{
    public static IServiceCollection AddFakeInstalledWormsArmageddonServices(
        this IServiceCollection builder,
        MockFileSystem mockFileSystem,
        string? gamePath = null,
        Version? version = null,
        bool hostCreatesReplay = true)
    {
        var installed = new Installed(mockFileSystem, gamePath, version, hostCreatesReplay);
        return builder.AddScoped<IFileSystem>(_ => mockFileSystem).AddScoped<IWormsArmageddon>(_ => installed);
    }

    public static IServiceCollection AddFakeNotInstalledWormsArmageddonServices(this IServiceCollection builder) =>
        builder.AddScoped<IWormsArmageddon, NotInstalled>();
}

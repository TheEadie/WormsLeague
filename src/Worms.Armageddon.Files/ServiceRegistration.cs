using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Worms.Armageddon.Files.Replays.Filename;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Armageddon.Files.Replays.Text.Parsers;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Random;
using JetBrains.Annotations;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Armageddon.Files;

[PublicAPI]
public static class ServiceRegistration
{
    public static IServiceCollection AddWormsArmageddonFilesServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFileSystem, FileSystem>();
        services.TryAddScoped<IWscReader, WscReader>();
        services.TryAddScoped<IWscWriter, WscWriter>();
        services.TryAddScoped<ISchemeTextReader, SchemeTextReader>();
        services.TryAddScoped<ISchemeTextWriter, SchemeTextWriter>();
        services.TryAddScoped<IReplayFilenameParser, ReplayFilenameParser>();
        services.TryAddScoped<IReplayTextReader, ReplayTextReader>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IReplayLineParser, StartTimeParser>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IReplayLineParser, TeamParser>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IReplayLineParser, WinnerParser>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IReplayLineParser, StartOfTurnParser>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IReplayLineParser, WeaponUsedParser>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IReplayLineParser, DamageParser>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IReplayLineParser, EndOfTurnParser>());
        services.TryAddScoped<IRandomSchemeGenerator, RandomSchemeGenerator>();
        return services;
    }
}

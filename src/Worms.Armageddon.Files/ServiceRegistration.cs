using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Files.Replays.Filename;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Armageddon.Files.Replays.Text.Parsers;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Random;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Armageddon.Files;

public static class ServiceRegistration
{
    public static IServiceCollection AddWormsArmageddonFilesServices(this IServiceCollection builder) =>
        builder.AddScoped<IWscReader, WscReader>()
            .AddScoped<IWscWriter, WscWriter>()
            .AddScoped<ISchemeTextReader, SchemeTextReader>()
            .AddScoped<ISchemeTextWriter, SchemeTextWriter>()
            .AddScoped<IReplayFilenameParser, ReplayFilenameParser>()
            .AddScoped<IReplayTextReader, ReplayTextReader>()
            .AddScoped<IReplayLineParser, StartTimeParser>()
            .AddScoped<IReplayLineParser, TeamParser>()
            .AddScoped<IReplayLineParser, WinnerParser>()
            .AddScoped<IReplayLineParser, StartOfTurnParser>()
            .AddScoped<IReplayLineParser, WeaponUsedParser>()
            .AddScoped<IReplayLineParser, DamageParser>()
            .AddScoped<IReplayLineParser, EndOfTurnParser>()
            .AddScoped<IRandomSchemeGenerator, RandomSchemeGenerator>();
}

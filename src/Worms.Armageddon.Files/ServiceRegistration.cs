using Microsoft.Extensions.DependencyInjection;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Armageddon.Files.Replays.Text.Parsers;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Armageddon.Files;

public static class ServiceRegistration
{
    public static IServiceCollection AddWormsArmageddonFilesServices(this IServiceCollection builder) =>
        builder.AddTransient<IWscReader, WscReader>()
            .AddTransient<IWscWriter, WscWriter>()
            .AddTransient<ISchemeTextReader, SchemeTextReader>()
            .AddTransient<ISchemeTextWriter, SchemeTextWriter>()
            .AddTransient<IReplayTextReader, ReplayTextReader>()
            .AddTransient<IReplayLineParser, StartTimeParser>()
            .AddTransient<IReplayLineParser, TeamParser>()
            .AddTransient<IReplayLineParser, WinnerParser>()
            .AddTransient<IReplayLineParser, StartOfTurnParser>()
            .AddTransient<IReplayLineParser, WeaponUsedParser>()
            .AddTransient<IReplayLineParser, DamageParser>()
            .AddTransient<IReplayLineParser, EndOfTurnParser>();
}

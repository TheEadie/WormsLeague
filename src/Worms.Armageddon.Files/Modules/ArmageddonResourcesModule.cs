using Autofac;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Armageddon.Files.Replays.Text.Parsers;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Armageddon.Files.Modules;

public class ArmageddonResourcesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Schemes
        _ = builder.RegisterType<WscReader>().As<IWscReader>();
        _ = builder.RegisterType<WscWriter>().As<IWscWriter>();
        _ = builder.RegisterType<SchemeTextReader>().As<ISchemeTextReader>();
        _ = builder.RegisterType<SchemeTextWriter>().As<ISchemeTextWriter>();

        // Replays
        _ = builder.RegisterType<ReplayTextReader>().As<IReplayTextReader>();
        _ = builder.RegisterType<StartTimeParser>().As<IReplayLineParser>();
        _ = builder.RegisterType<TeamParser>().As<IReplayLineParser>();
        _ = builder.RegisterType<WinnerParser>().As<IReplayLineParser>();
        _ = builder.RegisterType<StartOfTurnParser>().As<IReplayLineParser>();
        _ = builder.RegisterType<WeaponUsedParser>().As<IReplayLineParser>();
        _ = builder.RegisterType<DamageParser>().As<IReplayLineParser>();
        _ = builder.RegisterType<EndOfTurnParser>().As<IReplayLineParser>();
    }
}

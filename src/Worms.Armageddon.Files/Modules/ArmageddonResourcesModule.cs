using Autofac;
using Worms.Armageddon.Files.Replays.Text;
using Worms.Armageddon.Files.Replays.Text.Parsers;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Armageddon.Files.Modules
{
    public class ArmageddonResourcesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Schemes
            builder.RegisterType<WscReader>().As<IWscReader>();
            builder.RegisterType<WscWriter>().As<IWscWriter>();
            builder.RegisterType<SchemeTextReader>().As<ISchemeTextReader>();
            builder.RegisterType<SchemeTextWriter>().As<ISchemeTextWriter>();

            // Replays
            builder.RegisterType<ReplayTextReader>().As<IReplayTextReader>();
            builder.RegisterType<StartTimeParser>().As<IReplayLineParser>();
            builder.RegisterType<TeamParser>().As<IReplayLineParser>();
            builder.RegisterType<WinnerParser>().As<IReplayLineParser>();
            builder.RegisterType<StartOfTurnParser>().As<IReplayLineParser>();
            builder.RegisterType<WeaponUsedParser>().As<IReplayLineParser>();
            builder.RegisterType<DamageParser>().As<IReplayLineParser>();
            builder.RegisterType<EndOfTurnParser>().As<IReplayLineParser>();
        }
    }
}

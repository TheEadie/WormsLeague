using Autofac;
using Worms.Resources.Replays.Text;
using Worms.Resources.Schemes.Binary;
using Worms.Resources.Schemes.Text;

namespace Worms.Armageddon.Resources.Modules
{
    public class WormsArmageddonResourcesModule : Module
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
        }
    }
}

using Autofac;
using Worms.Armageddon.Resources.Replays.Text;
using Worms.Armageddon.Resources.Schemes.Binary;
using Worms.Armageddon.Resources.Schemes.Text;

namespace Worms.Armageddon.Resources.Modules
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
        }
    }
}

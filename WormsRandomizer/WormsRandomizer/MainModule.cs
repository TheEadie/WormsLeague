using Autofac;
using WormsRandomizer.Config;
using WormsRandomizer.Flags;
using WormsRandomizer.Random;
using WormsRandomizer.WormsScheme;

namespace WormsRandomizer
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WscReader>().As<IWscReader>().InstancePerLifetimeScope();
            builder.RegisterType<WscWriter>().As<IWscWriter>().InstancePerLifetimeScope();
            builder.RegisterType<FlagParser>().As<IFlagParser>().InstancePerLifetimeScope();
            builder.RegisterType<WeaponSetConfigReader>().As<IWeaponSetConfigReader>().InstancePerLifetimeScope();
            builder.RegisterType<RandomizerApp>().As<IRandomizerApp>().InstancePerLifetimeScope();

            builder.RegisterType<XoRoShiRo128Plus>().As<IRng>();

            builder.RegisterType<SchemeRandomizer>().As<ISchemeRandomizer>();
            builder.RegisterType<RandomizerApp>().AsSelf();
        }
    }
}

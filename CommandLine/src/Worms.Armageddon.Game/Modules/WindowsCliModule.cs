using Autofac;
using Worms.Armageddon.Game.Windows;

namespace Worms.Armageddon.Game.Modules
{
    internal class WindowsCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SteamService>().As<ISteamService>();
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();
        }
    }
}

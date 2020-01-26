using Autofac;
using Worms.GameRunner;
using Worms.GameRunner.Windows;

namespace Worms
{
    public class WindowsCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SteamService>().As<ISteamService>();
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();
        }
    }
}

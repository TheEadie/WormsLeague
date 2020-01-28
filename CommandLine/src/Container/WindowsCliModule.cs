using Autofac;
using Worms.Configuration.SecureStorage;
using Worms.WormsArmageddon;
using Worms.WormsArmageddon.Windows;

namespace Worms.Container
{
    public class WindowsCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsCredentialStorage>().As<ICredentialStorage>();

            builder.RegisterType<SteamService>().As<ISteamService>();
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();
        }
    }
}

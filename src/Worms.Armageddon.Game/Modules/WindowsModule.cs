using System.Runtime.Versioning;
using Autofac;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game.Modules
{
    [SupportedOSPlatform("windows")]
    internal class WindowsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SteamService>().As<ISteamService>();
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();
        }
    }
}

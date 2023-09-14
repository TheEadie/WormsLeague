using System.Runtime.Versioning;
using Autofac;
using Worms.Armageddon.Game.Win;

namespace Worms.Armageddon.Game.Modules;

[SupportedOSPlatform("windows")]
internal sealed class WindowsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        _ = builder.RegisterType<SteamService>().As<ISteamService>();
        _ = builder.RegisterType<WormsLocator>().As<IWormsLocator>();
        _ = builder.RegisterType<WormsRunner>().As<IWormsRunner>();
    }
}

using Autofac;
using Worms.Armageddon.Game.Linux;

namespace Worms.Armageddon.Game.Modules;

internal sealed class LinuxModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        _ = builder.RegisterType<WormsLocator>().As<IWormsLocator>();
        _ = builder.RegisterType<WormsRunner>().As<IWormsRunner>();
    }
}

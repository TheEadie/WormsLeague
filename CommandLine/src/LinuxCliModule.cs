using Autofac;
using Worms.WormsArmageddon;
using Worms.WormsArmageddon.Linux;

namespace Worms
{
    public class LinuxCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();
        }
    }
}

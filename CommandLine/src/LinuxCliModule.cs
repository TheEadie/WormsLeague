using Autofac;
using Worms.GameRunner;
using worms.GameRunner.Linux;

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

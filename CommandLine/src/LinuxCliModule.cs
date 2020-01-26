using Autofac;
using Worms.Configuration.SecureStorage;
using Worms.WormsArmageddon;
using Worms.WormsArmageddon.Linux;

namespace Worms
{
    public class LinuxCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NoOpCredentialStorage>().As<ICredentialStorage>();

            builder.RegisterType<WormsLocator>().As<IWormsLocator>();
            builder.RegisterType<WormsRunner>().As<IWormsRunner>();
        }
    }
}

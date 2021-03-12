using Autofac;
using Worms.Configuration.SecureStorage;

namespace Worms.Container
{
    public class LinuxCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NoOpCredentialStorage>().As<ICredentialStorage>();
        }
    }
}

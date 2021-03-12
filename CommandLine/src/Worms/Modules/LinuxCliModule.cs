using Autofac;
using Worms.Configuration.SecureStorage;

namespace Worms.Modules
{
    public class LinuxCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NoOpCredentialStorage>().As<ICredentialStorage>();
        }
    }
}

using Autofac;
using Worms.Configuration.SecureStorage;

namespace Worms.Modules
{
    internal class LinuxModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NoOpCredentialStorage>().As<ICredentialStorage>();
        }
    }
}

using Autofac;
using Worms.Configuration.SecureStorage;

namespace Worms.Container
{
    public class WindowsCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsCredentialStorage>().As<ICredentialStorage>();
        }
    }
}

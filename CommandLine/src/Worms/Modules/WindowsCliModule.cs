using Autofac;
using Worms.Configuration.SecureStorage;

namespace Worms.Modules
{
    public class WindowsCliModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsCredentialStorage>().As<ICredentialStorage>();
        }
    }
}

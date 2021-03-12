using Autofac;
using Worms.Configuration.SecureStorage;

namespace Worms.Modules
{
    internal class WindowsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsCredentialStorage>().As<ICredentialStorage>();
        }
    }
}

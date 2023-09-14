using Autofac;
using Worms.Cli.Configuration.SecureStorage;

namespace Worms.Cli.Modules;

internal class WindowsModule : Module
{
    protected override void Load(ContainerBuilder builder) => builder.RegisterType<WindowsCredentialStorage>().As<ICredentialStorage>();
}

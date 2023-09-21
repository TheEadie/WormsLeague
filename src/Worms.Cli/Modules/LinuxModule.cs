using Autofac;
using Worms.Cli.Configuration.SecureStorage;

namespace Worms.Cli.Modules;

internal sealed class LinuxModule : Module
{
    protected override void Load(ContainerBuilder builder) => builder.RegisterType<NoOpCredentialStorage>().As<ICredentialStorage>();
}

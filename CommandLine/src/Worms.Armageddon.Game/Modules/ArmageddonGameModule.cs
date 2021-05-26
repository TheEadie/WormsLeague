using System.Runtime.InteropServices;
using Autofac;
using Worms.Armageddon.Game.Replays;

namespace Worms.Armageddon.Game.Modules
{
    public class ArmageddonGameModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterOsModules(builder);

            // Replays
            builder.RegisterType<ReplayLogGenerator>().As<IReplayLogGenerator>();
            builder.RegisterType<ReplayPlayer>().As<IReplayPlayer>();
        }

        private static void RegisterOsModules(ContainerBuilder builder)
        {
            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.RegisterModule<WindowsModule>();
            }

            // Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                builder.RegisterModule<LinuxModule>();
            }
        }
    }
}

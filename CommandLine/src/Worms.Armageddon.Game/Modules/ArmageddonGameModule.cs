using System.IO.Abstractions;
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

            // FileSystem
            builder.RegisterType<FileSystem>().As<IFileSystem>();

            // Replays
            builder.RegisterType<ReplayLocator>().As<IReplayLocator>();
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

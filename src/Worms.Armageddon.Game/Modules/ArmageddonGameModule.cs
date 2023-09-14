using System.Runtime.InteropServices;
using Autofac;
using Worms.Armageddon.Game.Replays;

namespace Worms.Armageddon.Game.Modules;

public class ArmageddonGameModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterOsModules(builder);

        // Replays
        _ = builder.RegisterType<ReplayFrameExtractor>().As<IReplayFrameExtractor>();
        _ = builder.RegisterType<ReplayLogGenerator>().As<IReplayLogGenerator>();
        _ = builder.RegisterType<ReplayPlayer>().As<IReplayPlayer>();
    }

    private static void RegisterOsModules(ContainerBuilder builder)
    {
        // Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _ = builder.RegisterModule<WindowsModule>();
        }

        // Linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _ = builder.RegisterModule<LinuxModule>();
        }
    }
}

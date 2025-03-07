using System.Runtime.InteropServices;

namespace Worms.Armageddon.Game.Fake;

internal sealed class Installed : IWormsArmageddon
{
    public GameInfo FindInstallation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new GameInfo(
                true,
                @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\WA.exe",
                "WA",
                new Version(3, 8, 1, 0),
                @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Schemes",
                @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Games",
                @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Capture");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var linuxUserHome = Environment.GetEnvironmentVariable("HOME");
            return new GameInfo(
                true,
                $"{linuxUserHome}/.wine/drive_c/WA/WA.exe",
                "WA",
                new Version(3, 8, 1, 0),
                $"{linuxUserHome}/.wine/drive_c/WA/User/Schemes",
                $"{linuxUserHome}/.wine/drive_c/WA/User/Games",
                $"{linuxUserHome}/.wine/drive_c/WA/User/Capture");
        }

        throw new PlatformNotSupportedException($"Platform {RuntimeInformation.OSDescription} is not supported");
    }

    public Task Host() => Task.CompletedTask;

    public Task GenerateReplayLog(string replayPath) => throw new NotImplementedException();

    public Task PlayReplay(string replayPath) => throw new NotImplementedException();

    public Task PlayReplay(string replayPath, TimeSpan startTime) => throw new NotImplementedException();

    public Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480) =>
        throw new NotImplementedException();
}

using System.Runtime.Versioning;

namespace Worms.Armageddon.Game.Win;

[SupportedOSPlatform("windows")]
internal sealed class WormsLocator(IRegistry registry, IFileVersionInfo fileVersionInfo) : IWormsLocator
{
    public GameInfo Find()
    {
        const string processName = "WA";

        var location = registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null);

        if (location is null or "")
        {
            return GameInfo.NotInstalled;
        }

        var exeLocation = Path.Combine(location, processName + ".exe");
        var schemesFolder = Path.Combine(location, "User", "Schemes");
        var gamesFolder = Path.Combine(location, "User", "Games");
        var captureFolder = Path.Combine(location, "User", "Capture");

        var version = fileVersionInfo.GetVersionInfo(exeLocation);

        return new GameInfo(true, exeLocation, processName, version, schemesFolder, gamesFolder, captureFolder);
    }
}

using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Worms.Armageddon.Game.Win;

[SupportedOSPlatform("windows")]
internal sealed class WormsLocator : IWormsLocator
{
    public GameInfo Find()
    {
        const string processName = "WA";

        var location = Registry.GetValue(
            @"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon",
            "Path",
            null);

        if (location is not string rootLocation)
        {
            return GameInfo.NotInstalled;
        }

        var exeLocation = Path.Combine(rootLocation, processName + ".exe");
        var schemesFolder = Path.Combine(rootLocation, "User", "Schemes");
        var gamesFolder = Path.Combine(rootLocation, "User", "Games");
        var captureFolder = Path.Combine(rootLocation, "User", "Capture");

        var versionInfo = FileVersionInfo.GetVersionInfo(exeLocation);
        var version = new Version(
            versionInfo.ProductMajorPart,
            versionInfo.ProductMinorPart,
            versionInfo.ProductBuildPart,
            versionInfo.ProductPrivatePart);

        return new GameInfo(true, exeLocation, processName, version, schemesFolder, gamesFolder, captureFolder);
    }
}

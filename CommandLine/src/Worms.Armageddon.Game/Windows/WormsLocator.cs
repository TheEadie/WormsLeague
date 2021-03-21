using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Worms.Armageddon.Game.Windows
{
    [SupportedOSPlatform("windows")]
    internal class WormsLocator : IWormsLocator
    {
        public GameInfo Find()
        {
            const string processName = "WA";

            var location = Registry.GetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon",
                "Path",
                null);

            if (location is null || !(location is string rootLocation))
            {
                return GameInfo.NotInstalled;
            }

            var exeLocation = Path.Combine(rootLocation, processName + ".exe");
            var schemesFolder = Path.Combine(rootLocation, "User", "Schemes");
            var gamesFolder = Path.Combine(rootLocation, "User", "Games");

            var versionInfo = FileVersionInfo.GetVersionInfo(exeLocation);
            var version = new Version(
                versionInfo.ProductMajorPart,
                versionInfo.ProductMinorPart,
                versionInfo.ProductBuildPart,
                versionInfo.ProductPrivatePart);

            return new GameInfo(true, exeLocation, processName, version, schemesFolder, gamesFolder);
        }
    }
}

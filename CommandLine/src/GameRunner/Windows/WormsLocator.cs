using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace Worms.GameRunner.Windows
{
    internal class WormsLocator : IWormsLocator
    {
        public GameInfo Find()
        {
            const string processName = "WA";

            var location = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null);

            if (location is null)
            {
                return new GameInfo(false, string.Empty, string.Empty, new Version(0, 0, 0, 0));
            }

            var rootLocation = location as string;
            var exeLocation = Path.Combine(rootLocation, processName + ".exe");

            var versionInfo = FileVersionInfo.GetVersionInfo(exeLocation);
            var version = new Version(versionInfo.ProductMajorPart, versionInfo.ProductMinorPart, versionInfo.ProductBuildPart, versionInfo.ProductPrivatePart);

            return new GameInfo(true, exeLocation, processName, version);
        }
    }
}
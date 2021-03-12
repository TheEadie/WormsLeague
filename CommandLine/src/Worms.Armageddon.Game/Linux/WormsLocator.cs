using System;
using System.Diagnostics;
using System.IO;

namespace Worms.Armageddon.Game.Linux
{
    internal class WormsLocator : IWormsLocator
    {
        public GameInfo Find()
        {
            const string processName = "WA";
            var userHomeDirectory = Environment.GetEnvironmentVariable("HOME");

            if (string.IsNullOrEmpty(userHomeDirectory))
            {
                return GameInfo.NotInstalled;
            }

            var rootLocation = Path.Combine(userHomeDirectory, "games", "worms");
            var exeLocation = Path.Combine(rootLocation, processName + ".exe");
            var schemesFolder = Path.Combine(rootLocation, "User", "Schemes");
            var gamesFolder = Path.Combine(rootLocation, "User", "Games");

            if (!File.Exists(exeLocation))
            {
                return GameInfo.NotInstalled;
            }

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

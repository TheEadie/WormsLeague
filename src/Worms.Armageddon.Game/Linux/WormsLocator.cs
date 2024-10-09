using System.Diagnostics;

namespace Worms.Armageddon.Game.Linux;

internal sealed class WormsLocator : IWormsLocator
{
    public GameInfo Find()
    {
        const string processName = "WA";
        var userHomeDirectory = Environment.GetEnvironmentVariable("HOME");

        if (string.IsNullOrEmpty(userHomeDirectory))
        {
            return GameInfo.NotInstalled;
        }

        var rootLocation = Path.Combine(userHomeDirectory, ".wine", "drive_c", "WA");
        var exeLocation = Path.Combine(rootLocation, processName + ".exe");
        var schemesFolder = Path.Combine(rootLocation, "User", "Schemes");
        var gamesFolder = Path.Combine(rootLocation, "User", "Games");
        var captureFolder = Path.Combine(rootLocation, "User", "Capture");

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

        return new GameInfo(true, exeLocation, processName, version, schemesFolder, gamesFolder, captureFolder);
    }
}

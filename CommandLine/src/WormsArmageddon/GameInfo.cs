using System;

namespace Worms.WormsArmageddon
{
    public class GameInfo
    {
        public bool IsInstalled { get; }
        public string ExeLocation { get; }
        public string ProcessName { get; }
        public Version Version { get; }
        public string SchemesFolder { get; }

        public GameInfo(bool isInstalled, string exeLocation, string processName, Version version, string schemesFolder)
        {
            IsInstalled = isInstalled;
            ExeLocation = exeLocation;
            ProcessName = processName;
            Version = version;
            SchemesFolder = schemesFolder;
        }

        public static GameInfo NotInstalled = new GameInfo(
            false,
            string.Empty,
            string.Empty,
            new Version(0, 0),
            string.Empty);
    }
}

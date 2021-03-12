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
        public string ReplayFolder { get; }

        public GameInfo(bool isInstalled, string exeLocation, string processName, Version version, string schemesFolder, string replayFolder)
        {
            IsInstalled = isInstalled;
            ExeLocation = exeLocation;
            ProcessName = processName;
            Version = version;
            SchemesFolder = schemesFolder;
            ReplayFolder = replayFolder;
        }

        public static readonly GameInfo NotInstalled = new GameInfo(
            false,
            string.Empty,
            string.Empty,
            new Version(0, 0),
            string.Empty,
            string.Empty);
    }
}

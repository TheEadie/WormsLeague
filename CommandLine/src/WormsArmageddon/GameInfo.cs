using System;

namespace Worms.WormsArmageddon
{
    public class GameInfo
    {
        public bool IsInstalled { get; }
        public string ExeLocation { get; }
        public string ProcessName { get; }
        public Version Version { get; }

        public GameInfo(bool isInstalled, string exeLocation, string processName, Version version)
        {
            IsInstalled = isInstalled;
            ExeLocation = exeLocation;
            ProcessName = processName;
            Version = version;
        }
    }
}
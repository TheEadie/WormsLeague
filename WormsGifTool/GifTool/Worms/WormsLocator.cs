using System.IO;
using Microsoft.Win32;

namespace GifTool.Worms
{
    internal class WormsLocator : IWormsLocator
    {
        public string VideoLocation => Path.Combine(_rootLocation, "User/Capture");
        public string GamesLocation => Path.Combine(_rootLocation, "User/Games");
        public string ExeLocation => Path.Combine(_rootLocation, ProcessName + ".exe");
        public string ProcessName => "WA";

        private readonly string _rootLocation;

        public WormsLocator()
        {
            var location = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Team17SoftwareLTD\WormsArmageddon", "Path", null);
            _rootLocation = location as string ?? Directory.GetCurrentDirectory();
        }
    }
}
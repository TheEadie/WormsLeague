namespace Worms.Armageddon.Game.Replays
{
    public class ReplayPaths
    {
        public string WAgamePath { get; }
        public string LogPath { get; }

        public ReplayPaths(string waGamePath, string logPath)
        {
            WAgamePath = waGamePath;
            LogPath = logPath;
        }
    }
}

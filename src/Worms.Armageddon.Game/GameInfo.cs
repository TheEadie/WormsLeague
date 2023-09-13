namespace Worms.Armageddon.Game
{
    public record GameInfo(
        bool IsInstalled,
        string ExeLocation,
        string ProcessName,
        Version Version,
        string SchemesFolder,
        string ReplayFolder,
        string CaptureFolder)
    {
        public static readonly GameInfo NotInstalled = new GameInfo(
            false,
            string.Empty,
            string.Empty,
            new Version(0, 0),
            string.Empty,
            string.Empty,
            string.Empty);
    }
}

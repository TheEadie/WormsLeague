namespace Worms.Armageddon.Files.Replays.Text
{
    internal interface IReplayLineParser
    {
        public bool CanParse(string line);
        public void Parse(string line, ReplayResourceBuilder builder);
    }
}

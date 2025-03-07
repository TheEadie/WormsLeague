namespace Worms.Armageddon.Files.Replays.Text;

internal interface IReplayLineParser
{
    bool CanParse(string line);

    void Parse(string line, ReplayResourceBuilder builder);
}

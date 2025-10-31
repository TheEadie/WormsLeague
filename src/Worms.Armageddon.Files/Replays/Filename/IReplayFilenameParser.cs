namespace Worms.Armageddon.Files.Replays.Filename;

public interface IReplayFilenameParser
{
    ReplayFilenameInfo Parse(string filename);
}

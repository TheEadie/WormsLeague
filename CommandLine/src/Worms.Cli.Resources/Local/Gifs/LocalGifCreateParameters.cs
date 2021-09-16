using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Resources.Local.Gifs
{
    public record LocalGifCreateParameters(string Name, string Folder, LocalReplay Replay, int Turn);
}

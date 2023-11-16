using Worms.Armageddon.Game.Replays;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayViewer
    (IReplayPlayer replayPlayer) : IResourceViewer<LocalReplay, LocalReplayViewParameters>
{
    public async Task View(LocalReplay resource, LocalReplayViewParameters parameters)
    {
        if (parameters.Turn != default)
        {
            var startTime = resource.Details.Turns.ElementAt((int) parameters.Turn - 1).Start;
            await replayPlayer.Play(resource.Paths.WAgamePath, startTime);
            return;
        }

        await replayPlayer.Play(resource.Paths.WAgamePath);
    }
}

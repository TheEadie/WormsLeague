using Worms.Armageddon.Game;

namespace Worms.Cli.Resources.Local.Replays;

internal sealed class LocalReplayViewer(IWormsArmageddon wormsArmageddon)
    : IResourceViewer<LocalReplay, LocalReplayViewParameters>
{
    public async Task View(LocalReplay resource, LocalReplayViewParameters parameters)
    {
        if (parameters.Turn != default)
        {
            var startTime = resource.Details.Turns.ElementAt((int) parameters.Turn - 1).Start;
            await wormsArmageddon.PlayReplay(resource.Paths.WAgamePath, startTime);
            return;
        }

        await wormsArmageddon.PlayReplay(resource.Paths.WAgamePath);
    }
}

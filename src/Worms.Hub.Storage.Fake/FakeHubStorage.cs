using JetBrains.Annotations;

namespace Worms.Hub.Storage.Fake;

[PublicAPI]
public sealed class FakeHubStorage
{
    public FakeGamesRepository Games { get; } = new();
    public FakeReplaysRepository Replays { get; } = new();
    public FakeLeaguesRepository Leagues { get; } = new();
    public FakeRatingsRepository Ratings { get; } = new();
    public FakePlayersRepository Players { get; }
    public FakeTeamsRepository Teams { get; }

    public FakeHubStorage()
    {
        Players = new FakePlayersRepository();
        Teams = new FakeTeamsRepository(Players);
    }
}

using JetBrains.Annotations;
using Worms.Hub.Storage.Database;

namespace Worms.Hub.Storage.Fake;

public sealed class FakeLeaguesRepository : ILeaguesRepository
{
    private readonly List<LeagueDb> _store = [];
    private readonly Dictionary<(string Machine, string TeamName), IReadOnlyList<string>> _teamLeagues = [];

    [PublicAPI]
    public void Seed(params LeagueDb[] leagues)
    {
        _store.AddRange(leagues);
    }

    [PublicAPI]
    public void Seed(string machine, string teamName, params string[] leagueIds)
    {
        _teamLeagues[(machine, teamName)] = leagueIds;
    }

    public IReadOnlyList<LeagueDb> GetAll() => [.. _store];

    public LeagueDb? GetById(string id) => _store.FirstOrDefault(l => l.Id == id);

    public IReadOnlyList<string> GetLeaguesTeamPlaysIn(string machine, string teamName) =>
        _teamLeagues.TryGetValue((machine, teamName), out var leagues)
            ? leagues
            : [];
}

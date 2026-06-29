using JetBrains.Annotations;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Fake;

public sealed class FakeTeamsRepository : ITeamsRepository
{
    private sealed class TeamEntry
    {
        public int Id { get; init; }
        public string Machine { get; init; } = string.Empty;
        public string TeamName { get; init; } = string.Empty;
        public string? ClaimedByAuthSubject { get; set; }

        /// <summary>
        /// Explicitly seeded display name. When set, used verbatim at read time.
        /// Cleared when <see cref="ClaimedByAuthSubject"/> is updated via
        /// <see cref="FakeTeamsRepository.SetPlayerClaim"/>, so the name re-resolves from the
        /// players store after a claim operation.
        /// </summary>
        public string? SeededClaimedByPlayerName { get; set; }
    }

    private readonly FakePlayersRepository _players;
    private readonly List<TeamEntry> _store = [];
    private int _nextId = 1;

    public FakeTeamsRepository(FakePlayersRepository players)
    {
        ArgumentNullException.ThrowIfNull(players);
        _players = players;
    }

    [PublicAPI]
    public void Seed(params Team[] teams)
    {
        ArgumentNullException.ThrowIfNull(teams);
        foreach (var team in teams)
        {
            _store.Add(new TeamEntry
            {
                Id = team.Id,
                Machine = team.Machine,
                TeamName = team.TeamName,
                ClaimedByAuthSubject = team.ClaimedByAuthSubject,
                SeededClaimedByPlayerName = team.ClaimedByPlayerName
            });
            if (team.Id >= _nextId)
            {
                _nextId = team.Id + 1;
            }
        }
    }

    public IReadOnlyCollection<Team> GetAll() => [.. _store.Select(Project)];

    public Team? GetById(int id) =>
        _store.FirstOrDefault(t => t.Id == id) is { } entry ? Project(entry) : null;

    public void Upsert(string machine, string teamName)
    {
        if (_store.Any(t => t.Machine == machine && t.TeamName == teamName))
        {
            return; // ON CONFLICT DO NOTHING
        }

        _store.Add(new TeamEntry
        {
            Id = _nextId++,
            Machine = machine,
            TeamName = teamName,
            ClaimedByAuthSubject = null,
            SeededClaimedByPlayerName = null
        });
    }

    public void SetPlayerClaim(int teamId, string? authSubject)
    {
        var entry = _store.FirstOrDefault(t => t.Id == teamId);
        if (entry is null)
        {
            return;
        }

        entry.ClaimedByAuthSubject = authSubject;
        // Clear the seeded name so subsequent reads resolve from the players store
        entry.SeededClaimedByPlayerName = null;
    }

    private Team Project(TeamEntry entry)
    {
        string? claimedByPlayerName;
        if (entry.SeededClaimedByPlayerName is not null)
        {
            claimedByPlayerName = entry.SeededClaimedByPlayerName;
        }
        else if (entry.ClaimedByAuthSubject is not null)
        {
            claimedByPlayerName = _players.GetByAuthSubject(entry.ClaimedByAuthSubject)?.DisplayName;
        }
        else
        {
            claimedByPlayerName = null;
        }

        return new Team(
            entry.Id,
            entry.Machine,
            entry.TeamName,
            claimedByPlayerName,
            entry.ClaimedByAuthSubject);
    }
}

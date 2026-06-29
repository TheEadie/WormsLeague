using System.Globalization;
using JetBrains.Annotations;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Fake;

public sealed class FakeReplaysRepository : IReplaysRepository
{
    private readonly List<Replay> _store = [];
    private int _nextId = 1;

    [PublicAPI]
    public void Seed(params Replay[] replays)
    {
        ArgumentNullException.ThrowIfNull(replays);
        foreach (var replay in replays)
        {
            _store.Add(replay);
            if (int.TryParse(replay.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericId)
                && numericId >= _nextId)
            {
                _nextId = numericId + 1;
            }
        }
    }

    public IReadOnlyCollection<Replay> GetAll() => [.. _store];

    public Replay Create(Replay item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var newId = _nextId.ToString(CultureInfo.InvariantCulture);
        _nextId++;
        var stored = item with { Id = newId };
        _store.Add(stored);
        return stored;
    }

    public void Update(Replay item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var index = _store.FindIndex(r => r.Id == item.Id);
        if (index >= 0)
        {
            _store[index] = item;
        }
    }

    public IReadOnlyList<Replay> GetByLeagueId(string leagueId) =>
        // Mirror real repo's "ORDER BY date DESC NULLS LAST": dated replays newest-first, undated last.
        [.. _store
            .Where(r => r.LeagueId == leagueId)
            .OrderByDescending(r => r.Date.HasValue)
            .ThenByDescending(r => r.Date)];
}

using System.Globalization;
using JetBrains.Annotations;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Fake;

public sealed class FakeGamesRepository : IRepository<Game>
{
    private readonly List<Game> _store = [];
    private int _nextId = 1;

    [PublicAPI]
    public void Seed(params Game[] games)
    {
        ArgumentNullException.ThrowIfNull(games);
        foreach (var game in games)
        {
            _store.Add(game);
            if (int.TryParse(game.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericId)
                && numericId >= _nextId)
            {
                _nextId = numericId + 1;
            }
        }
    }

    public IReadOnlyCollection<Game> GetAll() => [.. _store];

    public Game Create(Game item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var newId = _nextId.ToString(CultureInfo.InvariantCulture);
        _nextId++;
        var stored = item with { Id = newId };
        _store.Add(stored);
        return stored;
    }

    public void Update(Game item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var index = _store.FindIndex(g => g.Id == item.Id);
        if (index >= 0)
        {
            _store[index] = item;
        }
    }
}

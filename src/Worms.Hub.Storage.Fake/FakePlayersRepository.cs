using JetBrains.Annotations;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Fake;

public sealed class FakePlayersRepository : IPlayersRepository
{
    // Use a list to allow players whose AuthSubject may be null (edge case: token with no sub claim).
    private readonly List<Player> _store = [];

    [PublicAPI]
    public void Seed(params Player[] players)
    {
        ArgumentNullException.ThrowIfNull(players);
        _store.AddRange(players);
    }

    /// <summary>Test-only snapshot of all seeded/created players.</summary>
    [PublicAPI]
    public IReadOnlyCollection<Player> All => [.. _store];

    public Player? GetByAuthSubject(string authSubject) =>
        _store.FirstOrDefault(p => p.AuthSubject == authSubject);

    public Player Create(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        // Replace any existing player with the same subject, or append
        var index = _store.FindIndex(p => p.AuthSubject == player.AuthSubject);
        if (index >= 0)
        {
            _store[index] = player;
        }
        else
        {
            _store.Add(player);
        }

        return player;
    }
}

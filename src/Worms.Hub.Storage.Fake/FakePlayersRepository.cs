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
        foreach (var player in players)
        {
            _ = Create(player);
        }
    }

    [PublicAPI]
    public IReadOnlyCollection<Player> All => [.. _store];

    public Player? GetByAuthSubject(string authSubject) =>
        _store.FirstOrDefault(p => p.AuthSubject == authSubject);

    public Player Create(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (_store.Exists(p => p.AuthSubject == player.AuthSubject))
        {
            throw new InvalidOperationException($"A player with auth subject '{player.AuthSubject}' already exists");
        }

        _store.Add(player);
        return player;
    }
}

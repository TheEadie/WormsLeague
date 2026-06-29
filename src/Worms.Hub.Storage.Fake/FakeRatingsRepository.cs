using JetBrains.Annotations;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Fake;

public sealed class FakeRatingsRepository : IRatingsRepository
{
    private readonly Dictionary<string, List<PlayerRating>> _store = [];

    [PublicAPI]
    public void Seed(string leagueId, params PlayerRating[] ratings)
    {
        _store[leagueId] = [.. ratings];
    }

    public IReadOnlyList<PlayerRating> GetByLeagueId(string leagueId) =>
        _store.TryGetValue(leagueId, out var ratings)
            ? [.. ratings.OrderByDescending(r => r.Rating)] // Mirror real repo's "ORDER BY rating DESC"
            : [];

    public void ReplaceForLeague(string leagueId, IReadOnlyList<PlayerRating> ratings)
    {
        ArgumentNullException.ThrowIfNull(ratings);
        _store[leagueId] = [.. ratings];
    }
}

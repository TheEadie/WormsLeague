using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface IRatingsRepository
{
    IReadOnlyList<PlayerRating> GetByLeagueId(string leagueId);
    void ReplaceForLeague(string leagueId, IReadOnlyList<PlayerRating> ratings);
}

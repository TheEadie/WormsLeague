using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface IReplaysRepositoryV04 : IRepository<Replay>
{
    IReadOnlyList<Replay> GetByLeagueId(string leagueId);
}

using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface IReplaysRepository
{
    IReadOnlyCollection<Replay> GetAll();
    Replay Create(Replay item);
    void Update(Replay item);
    IReadOnlyList<Replay> GetByLeagueId(string leagueId);
}

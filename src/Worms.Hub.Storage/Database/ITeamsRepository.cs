using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface ITeamsRepository
{
    IReadOnlyCollection<Team> GetAll();
    Team? GetById(int id);
    void Upsert(string machine, string teamName);
    void SetPlayerClaim(int teamId, string? authSubject);
}

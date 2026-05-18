using JetBrains.Annotations;

namespace Worms.Hub.Storage.Database;

[PublicAPI]
public interface ILeaguesRepository
{
    IReadOnlyList<LeagueDb> GetAll();
    LeagueDb? GetById(string id);
    IReadOnlyList<string> GetLeaguesTeamPlaysIn(string machine, string teamName);
}

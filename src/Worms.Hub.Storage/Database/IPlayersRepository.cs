using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface IPlayersRepository
{
    Player? GetByAuthSubject(string authSubject);
    Player Create(Player player);
}

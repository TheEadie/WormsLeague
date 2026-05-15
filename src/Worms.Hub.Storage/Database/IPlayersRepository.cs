using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

public interface IPlayersRepository
{
    Player? GetByAuth0Subject(string auth0Subject);
    Player Create(Player player);
}

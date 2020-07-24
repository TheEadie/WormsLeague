using System.Collections.Generic;

namespace Worms.DataAccess.Repositories
{
    public interface IGamesRepo
    {
        IReadOnlyCollection<Game> GetAll();

        Game Get(string id);

        void Add(Game game);
    }
}
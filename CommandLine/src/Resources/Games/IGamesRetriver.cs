using System.Collections.Generic;

namespace Worms.Resources.Games
{
    public interface IGameRetriever
    {
        IReadOnlyCollection<GameResource> Get(string pattern = "*");
    }
}

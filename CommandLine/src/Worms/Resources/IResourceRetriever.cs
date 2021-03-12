using System.Collections.Generic;

namespace Worms.Resources
{
    public interface IResourceRetriever<out T>
    {
        IReadOnlyCollection<T> Get(string pattern = "*");
    }
}

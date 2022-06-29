using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worms.Cli.Resources
{
    public interface IResourceRetriever<T>
    {
        Task<IReadOnlyCollection<T>> Get(string pattern = "*");
    }
}

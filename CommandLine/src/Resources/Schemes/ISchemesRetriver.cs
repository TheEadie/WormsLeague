using System.Collections.Generic;

namespace Worms.Resources.Schemes
{
    public interface ISchemesRetriever
    {
        IReadOnlyCollection<SchemeResource> Get();
        SchemeResource Get(string name);
    }
}
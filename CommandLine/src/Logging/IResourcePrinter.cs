using System.Collections.Generic;
using System.IO;

namespace Worms.Logging
{
    public interface IResourcePrinter<in T>
    {
        void Print(TextWriter writer, T resource);

        void Print(TextWriter writer, IReadOnlyCollection<T> resource);
    }
}

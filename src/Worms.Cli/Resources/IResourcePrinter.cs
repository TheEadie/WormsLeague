using System.Collections.Generic;
using System.IO;

namespace Worms.Cli.Resources
{
    public interface IResourcePrinter<in T>
    {
        void Print(TextWriter writer, T resource, int outputMaxWidth);

        void Print(TextWriter writer, IReadOnlyCollection<T> resource, int outputMaxWidth);
    }
}

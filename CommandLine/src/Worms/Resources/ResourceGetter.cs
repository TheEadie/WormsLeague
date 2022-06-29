using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Worms.Cli.Resources;
using Worms.Commands;

namespace Worms.Resources
{
    public class ResourceGetter<T>
    {
        private readonly IResourceRetriever<T> _retriever;
        private readonly IResourcePrinter<T> _printer;

        public ResourceGetter(IResourceRetriever<T> retriever, IResourcePrinter<T> printer)
        {
            _retriever = retriever;
            _printer = printer;
        }

        public async Task PrintResources(string name, TextWriter writer, int outputMaxWidth)
        {
            var requestForAll = string.IsNullOrWhiteSpace(name);
            var userSpecifiedName = !requestForAll && !name.Contains('*');
            var matches = requestForAll ? await _retriever.Get() : await _retriever.Get(name);

            if (userSpecifiedName)
            {
                switch (matches.Count)
                {
                    case 0:
                        throw new ConfigurationException($"No resources found with name: {name}");
                    case 1:
                        _printer.Print(writer, matches.Single(), outputMaxWidth);
                        break;
                    default:
                        _printer.Print(writer, matches, outputMaxWidth);
                        break;
                }
            }
            else
            {
                _printer.Print(writer, matches, outputMaxWidth);
            }
        }
    }
}

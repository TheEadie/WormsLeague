using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Commands;

namespace Worms.Cli.Resources
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

        public async Task PrintResources(string name, TextWriter writer, int outputMaxWidth, ILogger logger, CancellationToken cancellationToken)
        {
            var requestForAll = string.IsNullOrWhiteSpace(name);
            var userSpecifiedName = !requestForAll && !name.Contains('*');
            var matches = requestForAll ? await _retriever.Get(logger, cancellationToken) : await _retriever.Get(name, logger, cancellationToken);

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

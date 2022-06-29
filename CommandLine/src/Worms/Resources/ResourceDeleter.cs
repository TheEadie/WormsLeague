using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Resources;
using Worms.Commands;

namespace Worms.Resources
{
    public class ResourceDeleter<T>
    {
        private readonly IResourceRetriever<T> _retriever;
        private readonly IResourceDeleter<T> _deleter;

        public ResourceDeleter(IResourceRetriever<T> retriever, IResourceDeleter<T> deleter)
        {
            _retriever = retriever;
            _deleter = deleter;
        }

        public async Task Delete(string name, ILogger logger, CancellationToken cancellationToken)
        {
            name = ValidateName(name);
            var resource = await GetResource(name, logger, cancellationToken);
            _deleter.Delete(resource);
        }

        private async Task<T> GetResource(string name, ILogger logger, CancellationToken cancellationToken)
        {
            var resourcesFound = await _retriever.Get(name, logger, cancellationToken);

            if (resourcesFound.Count == 0)
            {
                throw new ConfigurationException($"No resource found with name: {name}");
            }

            if (resourcesFound.Count > 1)
            {
                throw new ConfigurationException($"More than one resource found with name matching: {name}");
            }

            return resourcesFound.Single();
        }

        private string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ConfigurationException("No name provided for the resource to be deleted.");
            }

            return name;
        }
    }
}

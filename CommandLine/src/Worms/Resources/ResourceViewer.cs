using System.Linq;
using System.Threading.Tasks;
using Worms.Cli.Resources;
using Worms.Commands;

namespace Worms.Resources
{
    public class ResourceViewer<T, TParams>
    {
        private readonly IResourceRetriever<T> _retriever;
        private readonly IResourceViewer<T, TParams> _resourceViewer;

        public ResourceViewer(IResourceRetriever<T> retriever, IResourceViewer<T, TParams> resourceViewer)
        {
            _retriever = retriever;
            _resourceViewer = resourceViewer;
        }

        public async Task View(string name, TParams parameters)
        {
            name = ValidateName(name);
            var resource = GetResource(name);
            await _resourceViewer.View(resource, parameters);
        }

        private T GetResource(string name)
        {
            var resourcesFound = _retriever.Get(name);

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
                throw new ConfigurationException("No name provided for the resource to be viewed.");
            }

            return name;
        }
    }
}

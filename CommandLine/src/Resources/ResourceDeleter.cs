using System.Linq;
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

        public void Delete(string name)
        {
            name = ValidateName(name);
            var resource = GetResource(name);
            _deleter.Delete(resource);
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
                throw new ConfigurationException("No name provided for the resource to be deleted.");
            }

            return name;
        }
    }
}

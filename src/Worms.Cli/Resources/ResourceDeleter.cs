using Serilog;
using Worms.Cli.Commands;

namespace Worms.Cli.Resources;

public class ResourceDeleter<T>(IResourceRetriever<T> retriever, IResourceDeleter<T> deleter)
{
    public async Task Delete(string name, ILogger logger, CancellationToken cancellationToken)
    {
        name = ValidateName(name);
        var resource = await GetResource(name, logger, cancellationToken).ConfigureAwait(false);
        deleter.Delete(resource);
    }

    private async Task<T> GetResource(string name, ILogger logger, CancellationToken cancellationToken)
    {
        var resourcesFound = await retriever.Retrieve(name, logger, cancellationToken).ConfigureAwait(false);

        return resourcesFound.Count == 0
            ? throw new ConfigurationException($"No resource found with name: {name}")
            :
            resourcesFound.Count > 1
                ?
                throw new ConfigurationException($"More than one resource found with name matching: {name}")
                : resourcesFound.Single();
    }

    private static string ValidateName(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? throw new ConfigurationException("No name provided for the resource to be deleted.")
            : name;
}

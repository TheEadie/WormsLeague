using Worms.Cli.Commands;

namespace Worms.Cli.Resources;

public class ResourceDeleter<T>(IResourceRetriever<T> retriever, IResourceDeleter<T> deleter)
{
    public async Task Delete(string name, CancellationToken cancellationToken)
    {
        name = ValidateName(name);
        var resource = await GetResource(name, cancellationToken).ConfigureAwait(false);
        deleter.Delete(resource);
    }

    private async Task<T> GetResource(string name, CancellationToken cancellationToken)
    {
        var resourcesFound = await retriever.Retrieve(name, cancellationToken).ConfigureAwait(false);

        return resourcesFound.Count switch
        {
            0 => throw new ConfigurationException($"No resource found with name: {name}"),
            1 => resourcesFound.Single(),
            > 1 => throw new ConfigurationException($"More than one resource found with name matching: {name}"),
            _ => throw new ArgumentOutOfRangeException(nameof(name), "Unexpected number of resources found.")
        };
    }

    private static string ValidateName(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? throw new ConfigurationException("No name provided for the resource to be deleted.")
            : name;
}

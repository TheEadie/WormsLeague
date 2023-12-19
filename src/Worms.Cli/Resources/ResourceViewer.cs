using Serilog;
using Worms.Cli.Commands;

namespace Worms.Cli.Resources;

public class ResourceViewer<T, TParams>(IResourceRetriever<T> retriever, IResourceViewer<T, TParams> resourceViewer)
{
    public async Task View(string name, TParams parameters, ILogger logger, CancellationToken cancellationToken)
    {
        name = ValidateName(name);
        var resource = await GetResource(name, logger, cancellationToken).ConfigureAwait(false);
        await resourceViewer.View(resource, parameters).ConfigureAwait(false);
    }

    private async Task<T> GetResource(string name, ILogger logger, CancellationToken cancellationToken)
    {
        var resourcesFound = await retriever.Retrieve(name, logger, cancellationToken).ConfigureAwait(false);

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
            ? throw new ConfigurationException("No name provided for the resource to be viewed.")
            : name;
}

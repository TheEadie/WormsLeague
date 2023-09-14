using Serilog;
using Worms.Cli.Commands;

namespace Worms.Cli.Resources;

public class ResourceViewer<T, TParams>
{
    private readonly IResourceRetriever<T> _retriever;
    private readonly IResourceViewer<T, TParams> _resourceViewer;

    public ResourceViewer(IResourceRetriever<T> retriever, IResourceViewer<T, TParams> resourceViewer)
    {
        _retriever = retriever;
        _resourceViewer = resourceViewer;
    }

    public async Task View(string name, TParams parameters, ILogger logger, CancellationToken cancellationToken)
    {
        name = ValidateName(name);
        var resource = await GetResource(name, logger, cancellationToken);
        await _resourceViewer.View(resource, parameters);
    }

    private async Task<T> GetResource(string name, ILogger logger, CancellationToken cancellationToken)
    {
        var resourcesFound = await _retriever.Get(name, logger, cancellationToken);

        return resourcesFound.Count == 0
            ? throw new ConfigurationException($"No resource found with name: {name}")
            :
            resourcesFound.Count > 1
                ?
                throw new ConfigurationException($"More than one resource found with name matching: {name}")
                : resourcesFound.Single();
    }

    private string ValidateName(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? throw new ConfigurationException("No name provided for the resource to be viewed.")
            : name;
}

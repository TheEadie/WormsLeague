namespace Worms.Cli.Resources;

internal sealed class ResourceGetter<T>(IResourceRetriever<T> retriever, IResourcePrinter<T> printer)
{
    public async Task<Validated<IReadOnlyCollection<T>>> GetResources(string name, CancellationToken cancellationToken)
    {
        var requestForAll = string.IsNullOrWhiteSpace(name);
        var userSpecifiedName = !requestForAll && !name.Contains('*', StringComparison.InvariantCulture);

        var matches = requestForAll
            ? await retriever.Retrieve(cancellationToken).ConfigureAwait(false)
            : await retriever.Retrieve(name, cancellationToken).ConfigureAwait(false);

        return matches.Count == 0 && userSpecifiedName
            ? new Invalid<IReadOnlyCollection<T>>($"No resources found for '{name}'")
            : new Valid<IReadOnlyCollection<T>>(matches);
    }

    public void PrintResources(IReadOnlyCollection<T> resources, TextWriter writer, int outputMaxWidth)
    {
        if (resources.Count == 1)
        {
            printer.Print(writer, resources.Single(), outputMaxWidth);
        }
        else
        {
            printer.Print(writer, resources, outputMaxWidth);
        }
    }
}

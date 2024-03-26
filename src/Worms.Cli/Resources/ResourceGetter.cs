using Worms.Cli.Commands;

namespace Worms.Cli.Resources;

public class ResourceGetter<T>(IResourceRetriever<T> retriever, IResourcePrinter<T> printer)
{
    public async Task PrintResources(
        string name,
        TextWriter writer,
        int outputMaxWidth,
        CancellationToken cancellationToken)
    {
        var requestForAll = string.IsNullOrWhiteSpace(name);
        var userSpecifiedName = !requestForAll && !name.Contains('*', StringComparison.InvariantCulture);
        var matches = requestForAll
            ? await retriever.Retrieve(cancellationToken).ConfigureAwait(false)
            : await retriever.Retrieve(name, cancellationToken).ConfigureAwait(false);

        if (userSpecifiedName)
        {
            switch (matches.Count)
            {
                case 0:
                    throw new ConfigurationException($"No resources found with name: {name}");
                case 1:
                    printer.Print(writer, matches.Single(), outputMaxWidth);
                    break;
                default:
                    printer.Print(writer, matches, outputMaxWidth);
                    break;
            }
        }
        else
        {
            printer.Print(writer, matches, outputMaxWidth);
        }
    }
}

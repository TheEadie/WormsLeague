using Serilog;
using Worms.Cli.Commands;

namespace Worms.Cli.Resources;

public class ResourceGetter<T>(IResourceRetriever<T> retriever, IResourcePrinter<T> printer)
{
    public async Task PrintResources(
        string name,
        TextWriter writer,
        int outputMaxWidth,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var requestForAll = string.IsNullOrWhiteSpace(name);
        var userSpecifiedName = !requestForAll && !name.Contains('*', StringComparison.InvariantCulture);
        var matches = requestForAll
            ? await retriever.Retrieve(logger, cancellationToken).ConfigureAwait(false)
            : await retriever.Retrieve(name, logger, cancellationToken).ConfigureAwait(false);

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

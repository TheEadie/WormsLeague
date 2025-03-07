using Worms.Cli.Commands.Validation;

namespace Worms.Cli.Resources;

internal sealed class ResourceGetter<T>(IResourceRetriever<T> retriever, IResourcePrinter<T> printer)
{
    public async Task<Validated<IReadOnlyCollection<T>>> GetResources(string name, CancellationToken cancellationToken)
    {
        var requestForAll = string.IsNullOrWhiteSpace(name);

        var matches = requestForAll
            ? await retriever.Retrieve(cancellationToken)
            : await retriever.Retrieve(name, cancellationToken);

        return matches.Validate(ContainsAtLeastOneResultIfSearchTerm(name));
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

    private static List<ValidationRule<IReadOnlyCollection<T>>> ContainsAtLeastOneResultIfSearchTerm(string searchTerm)
    {
        var userSpecifiedName = !string.IsNullOrWhiteSpace(searchTerm)
            && !searchTerm.Contains('*', StringComparison.InvariantCulture);
        return Valid.Rules<IReadOnlyCollection<T>>()
            .MustNot(x => x.Count == 0 && userSpecifiedName, $"No resources found for {searchTerm}");
    }
}

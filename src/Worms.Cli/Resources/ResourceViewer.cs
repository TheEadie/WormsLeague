using Worms.Cli.Commands.Validation;

namespace Worms.Cli.Resources;

internal sealed class ResourceViewer<T, TParams>(
    IResourceRetriever<T> retriever,
    IResourceViewer<T, TParams> resourceViewer)
{
    public async Task<Validated<T>> GetResource(string name, CancellationToken cancellationToken) =>
        await name.Validate(NameIsNotEmpty())
            .Map(x => retriever.Retrieve(x, cancellationToken))
            .Validate(Only1ResourceFound(name))
            .Map(x => x.Single());

    public void View(T resource, TParams parameters) => resourceViewer.View(resource, parameters);

    private static List<ValidationRule<string>> NameIsNotEmpty() =>
        Valid.Rules<string>()
            .Must(x => !string.IsNullOrWhiteSpace(x), "No name provided for the resource to be viewed.");

    private static List<ValidationRule<IReadOnlyCollection<T>>> Only1ResourceFound(string? name) =>
        Valid.Rules<IReadOnlyCollection<T>>()
            .MustNot(x => x.Count == 0, $"No resource found with name: {name}")
            .MustNot(x => x.Count > 1, $"More than one resource found with name matching: {name}");
}

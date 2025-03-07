using Worms.Cli.Commands.Validation;

namespace Worms.Cli.Resources;

internal sealed class ResourceDeleter<T>(IResourceRetriever<T> retriever, IResourceDeleter<T> deleter)
{
    public async Task<Validated<T>> GetResource(string name, CancellationToken cancellationToken) =>
        await name.Validate(NameIsNotEmpty())
            .Map(x => retriever.Retrieve(x, cancellationToken))
            .Validate(Only1ResourceFound(name))
            .Map(x => x.Single());

    public void Delete(T resource) => deleter.Delete(resource);

    private static List<ValidationRule<string>> NameIsNotEmpty() =>
        Valid.Rules<string>()
            .Must(x => !string.IsNullOrWhiteSpace(x), "No name provided for the resource to be deleted.");

    private static List<ValidationRule<IReadOnlyCollection<T>>> Only1ResourceFound(string? name) =>
        Valid.Rules<IReadOnlyCollection<T>>()
            .MustNot(x => x.Count == 0, $"No resource found with name: {name}")
            .MustNot(x => x.Count > 1, $"More than one resource found with name matching: {name}");
}

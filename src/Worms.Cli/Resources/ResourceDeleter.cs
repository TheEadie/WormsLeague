using Worms.Cli.Commands.Validation;

namespace Worms.Cli.Resources;

internal class ResourceDeleter<T>(IResourceRetriever<T> retriever, IResourceDeleter<T> deleter)
{
    public async Task<Validated<T>> GetResource(string name, CancellationToken cancellationToken) =>
        await name.Validate(NameIsNotEmpty())
            .Map(x => retriever.Retrieve(x, cancellationToken))
            .Validate(Only1ResourceFound(name))
            .Map(x => x.Single())
            .ConfigureAwait(false);

    public void Delete(T resource) => deleter.Delete(resource);

    private static IEnumerable<ValidationRule<string>> NameIsNotEmpty() =>
        new RulesFor<string>().Must(
                x => !string.IsNullOrWhiteSpace(x),
                "No name provided for the replay to be deleted.")
            .Build();

    private static IEnumerable<ValidationRule<IReadOnlyCollection<T>>> Only1ResourceFound(string? name) =>
        new RulesFor<IReadOnlyCollection<T>>().MustNot(x => x.Count == 0, $"No resource found with name: {name}")
            .MustNot(x => x.Count > 1, $"More than one resource found with name matching: {name}")
            .Build();
}

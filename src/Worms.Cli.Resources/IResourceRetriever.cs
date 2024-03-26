namespace Worms.Cli.Resources;

public interface IResourceRetriever<T>
{
    Task<IReadOnlyCollection<T>> Retrieve(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<T>> Retrieve(string pattern, CancellationToken cancellationToken);
}

using JetBrains.Annotations;

namespace Worms.Cli.Resources;

[PublicAPI]
public interface IResourceRetriever<T>
{
    Task<IReadOnlyCollection<T>> Retrieve(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<T>> Retrieve(string pattern, CancellationToken cancellationToken);
}

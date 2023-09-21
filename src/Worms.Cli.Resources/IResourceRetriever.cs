using Serilog;

namespace Worms.Cli.Resources;

public interface IResourceRetriever<T>
{
    Task<IReadOnlyCollection<T>> Retrieve(ILogger logger, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<T>> Retrieve(string pattern, ILogger logger, CancellationToken cancellationToken);
}

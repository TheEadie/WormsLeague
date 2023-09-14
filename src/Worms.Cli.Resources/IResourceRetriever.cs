using Serilog;

namespace Worms.Cli.Resources;

public interface IResourceRetriever<T>
{
    Task<IReadOnlyCollection<T>> Get(ILogger logger, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<T>> Get(string pattern, ILogger logger, CancellationToken cancellationToken);
}

using JetBrains.Annotations;

namespace Worms.Cli.Resources;

[PublicAPI]
public interface IResourceCreator<T, in TParams>
{
    Task<T> Create(TParams parameters, CancellationToken cancellationToken);
}

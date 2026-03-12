using JetBrains.Annotations;

namespace Worms.Cli.Resources;

[PublicAPI]
public interface IResourceViewer<in TResource, in TParams>
{
    Task View(TResource resource, TParams parameters);
}

using JetBrains.Annotations;

namespace Worms.Cli.Resources;

[PublicAPI]
public interface IResourceDeleter<in T>
{
    void Delete(T resource);
}

namespace Worms.Cli.Resources;

public interface IResourceDeleter<in T>
{
    void Delete(T resource);
}

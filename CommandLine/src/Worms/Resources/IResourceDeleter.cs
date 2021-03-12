namespace Worms.Resources
{
    public interface IResourceDeleter<in T>
    {
        void Delete(T resource);
    }
}

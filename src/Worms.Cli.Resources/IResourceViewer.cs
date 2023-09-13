namespace Worms.Cli.Resources
{
    public interface IResourceViewer<in TResource, in TParams>
    {
        Task View(TResource resource, TParams parameters);
    }
}

namespace Worms.Cli.Resources
{
    public interface IResourceCreator<in TParams>
    {
        void Create(TParams parameters);
    }
}

using System.Threading.Tasks;

namespace Worms.Cli.Resources
{
    public interface IResourceCreator<in TParams>
    {
        Task Create(TParams parameters);
    }
}

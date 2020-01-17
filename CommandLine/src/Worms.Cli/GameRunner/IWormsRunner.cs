using System.Threading.Tasks;

namespace Worms.GameRunner
{
    public interface IWormsRunner
    {
        Task RunWorms(params string[] wormsArgs);
    }
}
using System.Threading.Tasks;

namespace Worms.Armageddon.Game
{
    public interface IWormsRunner
    {
        Task RunWorms(params string[] wormsArgs);
    }
}

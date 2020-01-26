using System.Threading.Tasks;

namespace Worms.WormsArmageddon
{
    public interface IWormsRunner
    {
        Task RunWorms(params string[] wormsArgs);
    }
}
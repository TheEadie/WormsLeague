using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Worms.Cli.Resources
{
    public interface IResourceCreator<T, in TParams>
    {
        Task<T> Create(TParams parameters, ILogger logger, CancellationToken cancellationToken);
    }
}

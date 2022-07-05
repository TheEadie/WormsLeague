using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Worms.Cli.Resources.Remote.Auth
{
    public interface ILoginService
    {
        Task RequestLogin(ILogger logger, CancellationToken cancellationToken);
    }
}
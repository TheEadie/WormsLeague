using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Worms.Server.Auth
{
    public interface ILoginService
    {
        Task RequestLogin(ILogger logger, CancellationToken cancellationToken);
    }
}

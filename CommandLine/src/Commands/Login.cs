using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Server.Auth;

namespace Worms.Commands
{
    [Command("login", "auth", Description = "Authenticate with a Worms League Server")]
    internal class Login : CommandBase
    {
        private readonly ILoginService _loginService;

        public Login(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            await _loginService.RequestLogin(Logger, CancellationToken.None);
            return 0;
        }
    }
}

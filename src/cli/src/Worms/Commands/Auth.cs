using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Cli.Resources.Remote.Auth;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("auth", "login", Description = "Authenticate with a Worms League Server")]
    internal class Auth : CommandBase
    {
        private readonly ILoginService _loginService;

        public Auth(ILoginService loginService)
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
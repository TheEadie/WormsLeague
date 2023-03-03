using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Commands
{
    internal class Auth : Command
    {
        public Auth() : base("auth", "Authenticate with a Worms League Server")
        {
            AddAlias("login");
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class AuthHandler : ICommandHandler
    {
        private readonly ILoginService _loginService;
        private readonly ILogger _logger;

        public AuthHandler(ILoginService loginService, ILogger logger)
        {
            _loginService = loginService;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            await _loginService.RequestLogin(_logger, CancellationToken.None);
            return 0;
        }
    }
}
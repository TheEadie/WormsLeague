using System.CommandLine;
using System.CommandLine.Invocation;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Commands;

internal sealed class Auth : Command
{
    public Auth()
        : base("auth", "Authenticate with a Worms League Server") =>
        AddAlias("login");
}

internal sealed class AuthHandler(ILoginService loginService) : ICommandHandler
{
    public int Invoke(InvocationContext context) =>
        Task.Run(async () => await InvokeAsync(context).ConfigureAwait(false)).GetAwaiter().GetResult();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        await loginService.RequestLogin(CancellationToken.None).ConfigureAwait(false);
        return 0;
    }
}

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Commands;

internal sealed class Auth : Command
{
    public Auth()
        : base("auth", "Authenticate with a Worms League Server") =>
        Aliases.Add("login");
}

internal sealed class AuthHandler(ILoginService loginService) : AsynchronousCommandLineAction
{
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        _ = Activity.Current?.SetTag("name", Telemetry.Spans.Auth.SpanName);
        await loginService.RequestLogin(CancellationToken.None);
        return 0;
    }
}

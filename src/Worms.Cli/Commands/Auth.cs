﻿using System.CommandLine;
using System.CommandLine.Invocation;
using Serilog;
using Worms.Cli.Resources.Remote.Auth;

namespace Worms.Cli.Commands;

internal sealed class Auth : Command
{
    public Auth()
        : base("auth", "Authenticate with a Worms League Server") =>
        AddAlias("login");
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class AuthHandler(ILoginService loginService, ILogger logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) => Task.Run(async () => await InvokeAsync(context)).Result;

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        await loginService.RequestLogin(logger, CancellationToken.None);
        return 0;
    }
}

namespace Worms.Armageddon.Game.System;

internal interface IProcess : IDisposable
{
    Task WaitForExitAsync(CancellationToken cancellationToken = default);

    void Kill(bool entireProcessTree);

    StreamReader? StandardOutput { get; }
    StreamReader? StandardError { get; }
    int ExitCode { get; }
}

namespace Worms.Armageddon.Game.System;

internal interface IProcess : IDisposable
{
    Task WaitForExitAsync();

    StreamReader StandardOutput { get; }
    StreamReader StandardError { get; }
    int ExitCode { get; }
}

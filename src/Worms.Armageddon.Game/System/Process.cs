namespace Worms.Armageddon.Game.System;

internal sealed class Process(global::System.Diagnostics.Process process) : IProcess
{
    public Task WaitForExitAsync() => process.WaitForExitAsync();

    public StreamReader StandardOutput { get; } = process.StandardOutput;
    public StreamReader StandardError { get; } = process.StandardError;
    public int ExitCode { get; } = process.ExitCode;

    public void Dispose() => process.Dispose();
}

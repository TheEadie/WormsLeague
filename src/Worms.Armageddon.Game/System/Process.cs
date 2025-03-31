namespace Worms.Armageddon.Game.System;

internal sealed class Process(global::System.Diagnostics.Process process) : IProcess
{
    public Task WaitForExitAsync() => process.WaitForExitAsync();

    public StreamReader StandardOutput => process.StandardOutput;
    public StreamReader StandardError => process.StandardError;
    public int ExitCode => process.ExitCode;

    public void Dispose() => process.Dispose();
}

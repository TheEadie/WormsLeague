namespace Worms.Armageddon.Game.System;

internal sealed class Process(global::System.Diagnostics.Process process) : IProcess
{
    public Task WaitForExitAsync(CancellationToken cancellationToken = default) =>
        process.WaitForExitAsync(cancellationToken);

    public void Kill(bool entireProcessTree) => process.Kill(entireProcessTree);

    public StreamReader StandardOutput => process.StandardOutput;
    public StreamReader StandardError => process.StandardError;
    public int ExitCode => process.ExitCode;

    public void Dispose() => process.Dispose();
}

using System.Diagnostics;

namespace Worms.Armageddon.Game.System;

internal class ProcessRunner : IProcessRunner
{
    public IProcess Start(string fileName, params string[] args) =>
        new Process(global::System.Diagnostics.Process.Start(fileName, string.Join(" ", args.ToList())));

    public async Task<IProcess?> FindProcess(string processName)
    {
        IProcess? process = null;
        for (var retryCount = 0; process is null && retryCount <= 5; retryCount++)
        {
            await Task.Delay(500);
            var foundProcess = global::System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();
            process = foundProcess is null ? null : new Process(foundProcess);
        }

        return process;
    }

    public async Task<IProcess?> FindProcess(string processName, TimeSpan timeout)
    {
        // Initial wait to allow the launcher process to exit before polling
        await Task.Delay(500);
        timeout -= TimeSpan.FromMilliseconds(500);

        IProcess? process = null;
        while (process is null && timeout.TotalMilliseconds > 0)
        {
            await Task.Delay(500);
            var foundProcess = global::System.Diagnostics.Process.GetProcessesByName(processName)
                .FirstOrDefault(p => !p.HasExited);
            process = foundProcess is null ? null : new Process(foundProcess);
            timeout -= TimeSpan.FromMilliseconds(500);
        }

        _ = Activity.Current?.SetTag(Telemetry.Spans.ProcessRunner.TimeToFindProcess, timeout.Milliseconds);
        return process;
    }

    public IProcess Start(ProcessStartInfo processStartInfo) =>
        new Process(global::System.Diagnostics.Process.Start(processStartInfo)!);
}

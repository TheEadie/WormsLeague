using System.Diagnostics;

namespace Worms.Armageddon.Game.System;

internal class ProcessRunner : IProcessRunner
{
    public IProcess Start(string fileName, params string[] args) =>
        new Process(global::System.Diagnostics.Process.Start(fileName, string.Join(" ", args.ToList())));

    public IProcess? FindProcess(string processName)
    {
        IProcess? process = null;
        for (var retryCount = 0; process is null && retryCount <= 5; retryCount++)
        {
            Thread.Sleep(500);
            var foundProcess = global::System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();
            process = foundProcess is null ? null : new Process(foundProcess);
        }

        return process;
    }

    public IProcess? FindProcess(string processName, TimeSpan timeout)
    {
        IProcess? process = null;
        while (process is null && timeout.TotalMilliseconds > 0)
        {
            Thread.Sleep(500);
            var foundProcess = global::System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();
            process = foundProcess is null ? null : new Process(foundProcess);
            timeout -= TimeSpan.FromMilliseconds(500);
        }

        _ = Activity.Current?.SetTag(Telemetry.Spans.ProcessRunner.TimeToFindProcess, timeout.Milliseconds);
        return process;
    }

    public IProcess Start(ProcessStartInfo processStartInfo) =>
        new Process(global::System.Diagnostics.Process.Start(processStartInfo)!);
}

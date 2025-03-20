using System.Diagnostics;

namespace Worms.Armageddon.Game.System;

internal class ProcessRunner : IProcessRunner
{
    public IProcess? Start(string fileName, params string[] args) =>
        new Process(global::System.Diagnostics.Process.Start(fileName, args));

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

    public IProcess Start(ProcessStartInfo processStartInfo) =>
        new Process(global::System.Diagnostics.Process.Start(processStartInfo)!);
}

using System.Diagnostics;

namespace Worms.Armageddon.Game.System;

internal class ProcessRunner : IProcessRunner
{
    public IProcess? Start(string fileName, string args) =>
        new Process(global::System.Diagnostics.Process.Start(fileName, args));

    public IProcess?[] GetProcessesByName(string processName) =>
        global::System.Diagnostics.Process.GetProcessesByName(processName)
            .Select(p => new Process(p))
            .ToArray<IProcess?>();

    public IProcess Start(ProcessStartInfo processStartInfo) =>
        new Process(global::System.Diagnostics.Process.Start(processStartInfo)!);
}

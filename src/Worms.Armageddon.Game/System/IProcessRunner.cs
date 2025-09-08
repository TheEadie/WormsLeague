using System.Diagnostics;

namespace Worms.Armageddon.Game.System;

internal interface IProcessRunner
{
    IProcess Start(string fileName, params string[] args);

    IProcess FindProcess(string processName);

    IProcess Start(ProcessStartInfo processStartInfo);
}

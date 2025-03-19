using System.Diagnostics;

namespace Worms.Armageddon.Game.System;

internal interface IProcessRunner
{
    IProcess? Start(string fileName, string args);

    IProcess?[] GetProcessesByName(string processName);

    IProcess? Start(ProcessStartInfo processStartInfo);
}

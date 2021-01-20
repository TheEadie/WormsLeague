using System;
using System.Threading.Tasks;

namespace Worms.WormsArmageddon.Linux
{
    internal class WormsRunner : IWormsRunner
    {
        public Task RunWorms(params string[] wormsArgs)
        {
            throw new PlatformNotSupportedException(
                    "Running Worm Armageddon on Linux is not currently supported");
        }
    }
}

using System;
using System.Threading.Tasks;
using Worms.GameRunner;

namespace worms.GameRunner.Linux
{
    internal class WormsRunner : IWormsRunner
    {
        public Task RunWorms(params string[] wormsArgs)
        {
            return Task.Factory.StartNew(() => throw new PlatformNotSupportedException());
        }
    }
}

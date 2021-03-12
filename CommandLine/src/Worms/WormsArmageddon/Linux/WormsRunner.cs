using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Worms.WormsArmageddon.Linux
{
    internal class WormsRunner : IWormsRunner
    {
        private readonly IWormsLocator _wormsLocator;

        public WormsRunner(IWormsLocator wormsLocator)
        {
            _wormsLocator = wormsLocator;
        }

        public Task RunWorms(params string[] wormsArgs)
        {
            return Task.Factory.StartNew(
                () =>
                    {
                        var gameInfo = _wormsLocator.Find();

                        var args = string.Join(" ", wormsArgs);
                        using var process = Process.Start("wine", gameInfo.ExeLocation + " " + args);
                        if (process == null)
                        {
                            return;
                        }
                        process.WaitForExit();
                    });
        }
    }
}

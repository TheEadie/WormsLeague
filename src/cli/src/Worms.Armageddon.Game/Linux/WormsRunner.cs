using System.Diagnostics;
using System.Threading.Tasks;

namespace Worms.Armageddon.Game.Linux
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
                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = "wine",
                            Arguments = gameInfo.ExeLocation + " " + args,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        using var process = Process.Start(processStartInfo);
                        if (process == null)
                        {
                            return;
                        }
                        process.WaitForExit();
                    });
        }
    }
}

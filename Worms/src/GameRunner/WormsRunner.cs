using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Worms.GameRunner
{
    internal class WormsRunner : IWormsRunner
    {
        private readonly IWormsLocator _wormsLocator;
        private readonly ISteamService _steamService;

        public WormsRunner(IWormsLocator wormsLocator, ISteamService steamService)
        {
            _wormsLocator = wormsLocator;
            _steamService = steamService;
        }

        public Task RunWorms(params string[] wormsArgs)
        {
            return Task.Factory.StartNew(() =>
            {
                var args = string.Join(" ", wormsArgs);
                using (var process = Process.Start(_wormsLocator.ExeLocation, args))
                {
                    if (process == null) { return; }

                    process.WaitForExit();
                    if (process.ExitCode == 0) { return; }
                }

                _steamService.WaitForSteamPrompt();

                var wormsProcess = Process.GetProcessesByName(_wormsLocator.ProcessName).FirstOrDefault();
                wormsProcess?.WaitForExit();

                return;
            });
        }
    }
}

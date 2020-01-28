using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Worms.WormsArmageddon.Windows
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
                var gameInfo = _wormsLocator.Find();

                var args = string.Join(" ", wormsArgs);
                using (var process = Process.Start(gameInfo.ExeLocation, args))
                {
                    if (process == null) { return; }

                    process.WaitForExit();
                    if (process.ExitCode == 0) { return; }
                }

                _steamService.WaitForSteamPrompt();

                var wormsProcess = Process.GetProcessesByName(gameInfo.ProcessName).FirstOrDefault();
                wormsProcess?.WaitForExit();

                return;
            });
        }
    }
}

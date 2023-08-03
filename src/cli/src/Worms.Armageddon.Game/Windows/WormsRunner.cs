using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Worms.Armageddon.Game.Windows
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
            return Task.Run(
                () =>
                    {
                        var gameInfo = _wormsLocator.Find();

                        var args = string.Join(" ", wormsArgs);
                        using (var process = Process.Start(gameInfo.ExeLocation, args))
                        {
                            if (process == null)
                            {
                                throw new InvalidOperationException("Unable to start worms process");
                            }

                            process.WaitForExit();
                        }

                        _steamService.WaitForSteamPrompt();

                        var wormsProcess = FindWormsProcess(gameInfo);
                        wormsProcess.WaitForExit();

                        return Task.CompletedTask;
                    });
        }

        private static Process FindWormsProcess(GameInfo gameInfo)
        {
            Process wormsProcess = null;
            var retryCount = 0;
            while (wormsProcess is null && retryCount <= 10)
            {
                wormsProcess = Process.GetProcessesByName(gameInfo.ProcessName).FirstOrDefault();
                retryCount++;
            }

            if (wormsProcess is null)
            {
                throw new InvalidOperationException("Unable to find worms process");
            }

            return wormsProcess;
        }
    }
}

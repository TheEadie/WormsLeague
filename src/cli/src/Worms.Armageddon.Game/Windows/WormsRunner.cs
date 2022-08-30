﻿using System.Diagnostics;
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
            var gameInfo = _wormsLocator.Find();

            var args = string.Join(" ", wormsArgs);
            using (var process = Process.Start(gameInfo.ExeLocation, args))
            {
                if (process == null)
                {
                    return Task.CompletedTask;
                }

                process.WaitForExit();
            }

            _steamService.WaitForSteamPrompt();

            var wormsProcess = Process.GetProcessesByName(gameInfo.ProcessName).FirstOrDefault();
            wormsProcess?.WaitForExit();

            return Task.CompletedTask;
        }
    }
}
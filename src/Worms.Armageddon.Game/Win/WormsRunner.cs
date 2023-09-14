using System.Diagnostics;

namespace Worms.Armageddon.Game.Win;

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
        while (wormsProcess is null && retryCount <= 5)
        {
            Thread.Sleep(500);
            wormsProcess = Process.GetProcessesByName(gameInfo.ProcessName).FirstOrDefault();
            retryCount++;
        }

        return wormsProcess ?? throw new InvalidOperationException("Unable to find worms process");
    }
}

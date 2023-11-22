using System.Diagnostics;

namespace Worms.Armageddon.Game.Win;

internal sealed class WormsRunner(IWormsLocator wormsLocator, ISteamService steamService) : IWormsRunner
{
    public Task RunWorms(params string[] wormsArgs)
    {
        return Task.Run(
            async () =>
                {
                    var gameInfo = wormsLocator.Find();

                    var args = string.Join(" ", wormsArgs);
                    using (var process = Process.Start(gameInfo.ExeLocation, args))
                    {
                        if (process == null)
                        {
                            throw new InvalidOperationException("Unable to start worms process");
                        }

                        await process.WaitForExitAsync().ConfigureAwait(false);
                    }

                    steamService.WaitForSteamPrompt();

                    var wormsProcess = FindWormsProcess(gameInfo);

                    if (wormsProcess is not null)
                    {
                        await wormsProcess.WaitForExitAsync().ConfigureAwait(false);
                    }

                    return Task.CompletedTask;
                });
    }

    private static Process? FindWormsProcess(GameInfo gameInfo)
    {
        Process? wormsProcess = null;
        var retryCount = 0;
        while (wormsProcess is null && retryCount <= 5)
        {
            Thread.Sleep(500);
            wormsProcess = Process.GetProcessesByName(gameInfo.ProcessName).FirstOrDefault();
            retryCount++;
        }

        return wormsProcess;
    }
}

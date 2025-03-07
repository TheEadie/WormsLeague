using System.Diagnostics;

namespace Worms.Armageddon.Game.Win;

internal sealed class WormsRunner(IWormsLocator wormsLocator, ISteamService steamService) : IWormsRunner
{
    public Task RunWorms(params string[] wormsArgs)
    {
        return Task.Run(
            async () =>
                {
                    using var span = Activity.Current?.Source.StartActivity(
                        Telemetry.Spans.WormsArmageddon.SpanName,
                        ActivityKind.Client);

                    var gameInfo = wormsLocator.Find();
                    var args = string.Join(" ", wormsArgs);

                    _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.Version, gameInfo.Version);
                    _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.Args, args);

                    using (var process = Process.Start(gameInfo.ExeLocation, args))
                    {
                        if (process == null)
                        {
                            _ = span?.SetStatus(ActivityStatusCode.Error);
                            throw new InvalidOperationException("Unable to start worms process");
                        }

                        await process.WaitForExitAsync();
                    }

                    steamService.WaitForSteamPrompt();

                    var wormsProcess = FindWormsProcess(gameInfo);

                    if (wormsProcess is not null)
                    {
                        await wormsProcess.WaitForExitAsync();
                    }

                    return Task.CompletedTask;
                });
    }

    private static Process? FindWormsProcess(GameInfo gameInfo)
    {
        Process? wormsProcess = null;
        for (var retryCount = 0; wormsProcess is null && retryCount <= 5; retryCount++)
        {
            Thread.Sleep(500);
            wormsProcess = Process.GetProcessesByName(gameInfo.ProcessName).FirstOrDefault();
        }

        return wormsProcess;
    }
}

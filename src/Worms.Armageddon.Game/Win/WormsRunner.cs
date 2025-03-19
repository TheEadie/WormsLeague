using System.Diagnostics;
using Worms.Armageddon.Game.System;

namespace Worms.Armageddon.Game.Win;

internal sealed class WormsRunner(IWormsLocator wormsLocator, ISteamService steamService, IProcessRunner processRunner)
    : IWormsRunner
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
                    if (!gameInfo.IsInstalled)
                    {
                        _ = span?.SetStatus(ActivityStatusCode.Error);
                        throw new InvalidOperationException("Worms Armageddon is not installed");
                    }

                    var args = string.Join(" ", wormsArgs);

                    _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.Version, gameInfo.Version);
                    _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.Args, args);

                    using (var process = processRunner.Start(gameInfo.ExeLocation, args))
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

    private IProcess? FindWormsProcess(GameInfo gameInfo)
    {
        IProcess? wormsProcess = null;
        for (var retryCount = 0; wormsProcess is null && retryCount <= 5; retryCount++)
        {
            Thread.Sleep(500);
            wormsProcess = processRunner.GetProcessesByName(gameInfo.ProcessName).FirstOrDefault();
        }

        return wormsProcess;
    }
}

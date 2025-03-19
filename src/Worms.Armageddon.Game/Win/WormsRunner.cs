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

                    _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.Version, gameInfo.Version);
                    _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.Args, wormsArgs);

                    using (var process = processRunner.Start(gameInfo.ExeLocation, wormsArgs))
                    {
                        if (process == null)
                        {
                            _ = span?.SetStatus(ActivityStatusCode.Error);
                            throw new InvalidOperationException("Unable to start worms process");
                        }

                        await process.WaitForExitAsync();
                    }

                    steamService.WaitForSteamPrompt();

                    var wormsProcess = processRunner.FindProcess(gameInfo.ProcessName);

                    if (wormsProcess is not null)
                    {
                        await wormsProcess.WaitForExitAsync();
                    }

                    return Task.CompletedTask;
                });
    }
}

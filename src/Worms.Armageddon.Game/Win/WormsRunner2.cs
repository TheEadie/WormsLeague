using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game.System;

namespace Worms.Armageddon.Game.Win;

internal sealed class WormsRunner2(
    IWormsLocator wormsLocator,
    IProcessRunner processRunner,
    ILogger<WormsRunner2> logger) : IWormsRunner
{
    public Task RunWorms(params string[] wormsArgs)
    {
        return Task.Run(async () =>
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

                logger.Log(LogLevel.Debug, "Running Worms Armageddon: {Path}", gameInfo.ExeLocation);
                logger.Log(LogLevel.Debug, "Args: {Arguments}", wormsArgs.ToList());

                _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.Version, gameInfo.Version);
                _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.Args, wormsArgs);

                using (processRunner.Start(gameInfo.ExeLocation, wormsArgs))
                {
                    var wormsProcess = FindWormsProcess(gameInfo.ProcessName);
                    await WaitForExit(wormsProcess);
                }
            });
    }

    private IProcess FindWormsProcess(string processName)
    {
        const int timeoutInMinutes = 5;
        logger.Log(LogLevel.Debug, "Looking for worms process: {ProcessName}...", processName);

        var wormsProcess = processRunner.FindProcess(processName, TimeSpan.FromMinutes(timeoutInMinutes));

        if (wormsProcess is null)
        {
            logger.Log(LogLevel.Error, "Unable to find worms process after {Minutes} minutes", timeoutInMinutes);
            _ = Activity.Current?.SetStatus(ActivityStatusCode.Error);
            throw new TimeoutException($"Unable to find worms process after {timeoutInMinutes} minutes");
        }

        logger.Log(LogLevel.Debug, "Process found");
        return wormsProcess;
    }

    private async Task WaitForExit(IProcess wormsProcess)
    {
        logger.Log(LogLevel.Debug, "Waiting for process to exit...");
        await wormsProcess.WaitForExitAsync();
        logger.Log(LogLevel.Debug, "Worms process exited with code: {ExitCode}", wormsProcess.ExitCode);
        _ = Activity.Current?.SetTag(Telemetry.Spans.WormsArmageddon.ExitCode, wormsProcess.ExitCode);
    }
}

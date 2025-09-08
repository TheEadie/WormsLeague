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
                    logger.Log(LogLevel.Debug, "Looking for worms process: {ProcessName}...", gameInfo.ProcessName);
                    var wormsProcess = processRunner.FindProcess(gameInfo.ProcessName);
                    logger.Log(LogLevel.Debug, "Process found");
                    logger.Log(LogLevel.Debug, "Waiting for process to exit...");
                    await wormsProcess.WaitForExitAsync();
                    logger.Log(LogLevel.Debug, "Worms process exited with code: {ExitCode}", wormsProcess.ExitCode);
                    _ = span?.SetTag(Telemetry.Spans.WormsArmageddon.ExitCode, wormsProcess.ExitCode);
                }
            });
    }
}

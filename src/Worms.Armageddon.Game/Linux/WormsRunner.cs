using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Worms.Armageddon.Game.System;

namespace Worms.Armageddon.Game.Linux;

internal sealed class WormsRunner(IWormsLocator wormsLocator, IProcessRunner processRunner, ILogger<WormsRunner> logger)
    : IWormsRunner
{
    public Task RunWorms(params string[] wormsArgs)
    {
        // Replace " with ' in the args
        // Wine doesn't like using "s around arguments
        // Windows doesn't like using 's around arguments
        for (var i = 0; i < wormsArgs.Length; i++)
        {
            wormsArgs[i] = wormsArgs[i].Replace("\"", "'", StringComparison.InvariantCulture);
        }

        return Task.Run(
            async () =>
                {
                    var gameInfo = wormsLocator.Find();
                    if (!gameInfo.IsInstalled)
                    {
                        throw new InvalidOperationException("Worms Armageddon is not installed");
                    }

                    var args = string.Join(" ", wormsArgs);

                    logger.Log(LogLevel.Debug, "Running Worms Armageddon: {Path}", gameInfo.ExeLocation);
                    logger.Log(LogLevel.Debug, "Args: {Arguments}", args);
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"""
                                     -c "xvfb-run wine "{gameInfo.ExeLocation}" {args}; wineserver -k"
                                     """,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using var process = processRunner.Start(processStartInfo);

                    var output = PrintStdOut(process.StandardOutput);
                    var errors = PrintStdErr(process.StandardError);

                    await process.WaitForExitAsync();
                    logger.Log(LogLevel.Debug, "Process exited with code: {ExitCode}", process.ExitCode);

                    // On Ubuntu 24, child processes (e.g. wineserver) can hold stdout/stderr
                    // pipes open after the main process exits. Wait briefly for output to
                    // drain, then move on rather than hanging indefinitely.
                    var readComplete = Task.WhenAll(output, errors);
                    if (await Task.WhenAny(readComplete, Task.Delay(TimeSpan.FromSeconds(10))) != readComplete)
                    {
                        logger.Log(LogLevel.Warning, "Timed out waiting for process output streams to close");
                    }

                    return Task.CompletedTask;
                });
    }

    private async Task PrintStdOut(StreamReader? reader)
    {
        if (reader is null)
        {
            return;
        }

        while (await reader.ReadLineAsync() is { } line)
        {
            logger.Log(LogLevel.Debug, "StdOut: {Message}", line);
        }
    }

    private async Task PrintStdErr(StreamReader? reader)
    {
        if (reader is null)
        {
            return;
        }

        while (await reader.ReadLineAsync() is { } line)
        {
            logger.Log(LogLevel.Debug, "StdErr: {Message}", line);
        }
    }
}

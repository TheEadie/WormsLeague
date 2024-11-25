using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Worms.Armageddon.Game.Linux;

internal sealed class WormsRunner(IWormsLocator wormsLocator, ILogger<WormsRunner> logger) : IWormsRunner
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
                                     -c "xvfb-run wine "{gameInfo.ExeLocation}" {args}"
                                     """,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = $"{gameInfo.ReplayFolder}",
                    };

                    using var process = Process.Start(processStartInfo);

                    if (process is not null)
                    {
                        var processTask = Task.Run(() => process.WaitForExitAsync());
                        var output = Task.Run(() => PrintStdOut(process));
                        var errors = Task.Run(() => PrintStdErr(process));

                        await Task.WhenAll(processTask, errors, output);
                        await Console.Error.WriteLineAsync("Exit code:" + process.ExitCode);
                    }

                    return Task.CompletedTask;
                });
    }

    private async Task PrintStdOut(Process process)
    {
        while (!process.StandardOutput.EndOfStream)
        {
            var line = await process.StandardOutput.ReadLineAsync();
            logger.Log(LogLevel.Debug, "StdOut: {Message}", line);
        }
    }

    private async Task PrintStdErr(Process process)
    {
        while (!process.StandardError.EndOfStream)
        {
            var line = await process.StandardError.ReadLineAsync();
            logger.Log(LogLevel.Debug, "StdErr: {Message}", line);
        }
    }
}

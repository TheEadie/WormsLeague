using System.Diagnostics;

namespace Worms.Armageddon.Game.Linux;

internal sealed class WormsRunner(IWormsLocator wormsLocator) : IWormsRunner
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
                    var args = string.Join(" ", wormsArgs);

                    Console.WriteLine("ARGS: " + args);
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

                        await Task.WhenAll(processTask, errors, output).ConfigureAwait(false);
                        await Console.Error.WriteLineAsync("Exit code:" + process.ExitCode).ConfigureAwait(false);
                    }

                    return Task.CompletedTask;
                });
    }

    private static async Task PrintStdOut(Process process)
    {
        while (!process.StandardOutput.EndOfStream)
        {
            var line = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            await Console.Error.WriteLineAsync("StdOut: " + line).ConfigureAwait(false);
        }
    }

    private static async Task PrintStdErr(Process process)
    {
        while (!process.StandardError.EndOfStream)
        {
            var line = await process.StandardError.ReadLineAsync().ConfigureAwait(false);
            await Console.Error.WriteLineAsync("StdErr: " + line).ConfigureAwait(false);
        }
    }
}

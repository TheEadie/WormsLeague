using System.Diagnostics;

namespace Worms.Armageddon.Game.Linux;

internal sealed class WormsRunner(IWormsLocator wormsLocator) : IWormsRunner
{
    public Task RunWorms(params string[] wormsArgs)
    {
        return Task.Run(
            async () =>
                {
                    var gameInfo = wormsLocator.Find();

                    var args = string.Join(" ", wormsArgs);
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "wine",
                        Arguments = gameInfo.ExeLocation + " " + args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = $"{gameInfo.ReplayFolder}",
                    };

                    processStartInfo.EnvironmentVariables["WINEDLLOVERRIDES"] = "mscoree,mshtml=";

                    using var process = Process.Start(processStartInfo);

                    if (process is not null)
                    {
                        await process.WaitForExitAsync().ConfigureAwait(false);
                        Console.WriteLine("Exit code:" + process.ExitCode);

                        Console.WriteLine("StdErr:");
                        (await process.StandardError.ReadToEndAsync().ConfigureAwait(false)).Split('\n')
                            .ToList()
                            .ForEach(Console.WriteLine);

                        Console.WriteLine("StdOut:");
                        while (!process.StandardOutput.EndOfStream)
                        {
                            var line = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false);
                            Console.WriteLine(line);
                        }
                    }

                    return Task.CompletedTask;
                });
    }
}

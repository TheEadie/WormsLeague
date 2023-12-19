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
                        RedirectStandardError = true
                    };

                    using var process = Process.Start(processStartInfo);

                    if (process is not null)
                    {
                        await process.WaitForExitAsync().ConfigureAwait(false);
                    }

                    return Task.CompletedTask;
                });
    }
}

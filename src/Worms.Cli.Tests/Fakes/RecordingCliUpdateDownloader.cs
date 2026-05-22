using System.IO.Abstractions;
using Worms.Cli.Resources.Remote.Updates;

namespace Worms.Cli.Tests.Fakes;

internal sealed class RecordingCliUpdateDownloader(IFileSystem fileSystem, string executableFileName)
    : ICliUpdateDownloader
{
    public List<string> Calls { get; } = [];

    public Task DownloadLatestCli(string updateFolder)
    {
        Calls.Add(updateFolder);
        var path = fileSystem.Path.Combine(updateFolder, executableFileName);
        fileSystem.File.WriteAllBytes(path, []);
        return Task.CompletedTask;
    }
}

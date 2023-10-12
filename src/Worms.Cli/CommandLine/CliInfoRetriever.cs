using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using Serilog;

namespace Worms.Cli.CommandLine;

internal sealed class CliInfoRetriever
{
    private readonly IFileSystem _fileSystem;

    public CliInfoRetriever(IFileSystem fileSystem) => _fileSystem = fileSystem;

    public CliInfo Get(ILogger logger)
    {
        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetName().Version;
        if (version is null)
        {
            logger.Warning("Could not get version of Worms CLI from assembly info");
            version = new Version(0, 0, 0);
        }

        var cliFolder = _fileSystem.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
        if (cliFolder is null)
        {
            logger.Warning("Could not get folder of Worms CLI from assembly info");
            cliFolder = string.Empty;
        }

        return new CliInfo(version, cliFolder);
    }
}

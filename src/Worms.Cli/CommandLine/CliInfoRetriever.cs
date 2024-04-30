using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.CommandLine;

internal sealed class CliInfoRetriever(IFileSystem fileSystem, ILogger<CliInfoRetriever> logger)
{
    public CliInfo Get()
    {
        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetName().Version;
        if (version is null)
        {
            logger.LogWarning("Could not get version of Worms CLI from assembly info");
            version = new Version(0, 0, 0);
        }

        var execPath = Process.GetCurrentProcess().MainModule?.FileName;
        var execName = fileSystem.Path.GetFileName(execPath);
        if (execName is null)
        {
            logger.LogWarning("Could not get executable file of Worms CLI from assembly info");
            execName = string.Empty;
        }

        var cliFolder = fileSystem.Path.GetDirectoryName(execPath);
        if (cliFolder is null)
        {
            logger.LogWarning("Could not get folder of Worms CLI from assembly info");
            cliFolder = string.Empty;
        }

        return new CliInfo(version, cliFolder, execName);
    }
}

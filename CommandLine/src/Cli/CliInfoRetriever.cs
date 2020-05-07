using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;

namespace Worms.Cli
{
    internal class CliInfoRetriever
    {
        private readonly IFileSystem _fileSystem;

        public CliInfoRetriever(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public CliInfo Get()
        {
            var assembly = Assembly.GetEntryAssembly();
            return new CliInfo(
                assembly?.GetName().Version,
                _fileSystem.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName));
        }
    }
}

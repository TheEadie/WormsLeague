using System.IO.Abstractions;
using Serilog;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Cli.Resources.Local.Schemes;

internal sealed class LocalSchemeCreator(
    ISchemeTextReader schemeTextReader,
    IWscWriter wscWriter,
    IFileSystem fileSystem) : IResourceCreator<LocalScheme, LocalSchemeCreateParameters>
{
    public Task<LocalScheme> Create(
        LocalSchemeCreateParameters parameters,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var scheme = schemeTextReader.GetModel(parameters.Definition);
        var path = fileSystem.Path.Combine(parameters.Folder, parameters.Name + ".wsc");
        wscWriter.Write(scheme, path);

        return Task.FromResult(new LocalScheme(path, parameters.Name, scheme));
    }
}

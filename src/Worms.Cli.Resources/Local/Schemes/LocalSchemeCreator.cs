using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Random;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Cli.Resources.Local.Schemes;

internal sealed class LocalSchemeCreator(
    ISchemeTextReader schemeTextReader,
    IRandomSchemeGenerator randomSchemeGenerator,
    IWscWriter wscWriter,
    IFileSystem fileSystem) : IResourceCreator<LocalScheme, LocalSchemeCreateParameters>
{
    public Task<LocalScheme> Create(LocalSchemeCreateParameters parameters, CancellationToken cancellationToken)
    {
        var scheme = GetScheme(parameters);
        var path = fileSystem.Path.Combine(parameters.Folder, parameters.Name + ".wsc");
        wscWriter.Write(scheme, path);

        return Task.FromResult(new LocalScheme(path, parameters.Name, scheme));
    }

    [SuppressMessage("Style", "IDE0046:Convert to conditional expression")]
    private Scheme GetScheme(LocalSchemeCreateParameters parameters)
    {
        if (parameters.Random)
        {
            return randomSchemeGenerator.Generate();
        }

        if (!string.IsNullOrWhiteSpace(parameters.Definition))
        {
            return schemeTextReader.GetModel(parameters.Definition);
        }

        throw new InvalidOperationException("No scheme definition provided");
    }
}

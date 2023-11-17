using System.IO.Abstractions;
using Serilog;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Random;

namespace Worms.Cli.Resources.Local.Schemes;

internal sealed class LocalSchemeRandomCreator(
    IRandomSchemeGenerator randomSchemeGenerator,
    IWscWriter wscWriter,
    IFileSystem fileSystem) : IResourceCreator<LocalScheme, LocalSchemeCreateRandomParameters>
{
    public Task<LocalScheme> Create(
        LocalSchemeCreateRandomParameters parameters,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var scheme = randomSchemeGenerator.Generate();
        var path = fileSystem.Path.Combine(parameters.Folder, parameters.Name + ".wsc");
        wscWriter.Write(scheme, path);

        return Task.FromResult(new LocalScheme(path, parameters.Name, scheme));
    }
}

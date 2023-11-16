using System.IO.Abstractions;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Armageddon.Files.Schemes.Binary;

namespace Worms.Cli.Resources.Local.Schemes;

internal sealed class LocalSchemesRetriever
    (IWscReader wscReader, IWormsLocator wormsLocator, IFileSystem fileSystem) : IResourceRetriever<LocalScheme>
{
    public Task<IReadOnlyCollection<LocalScheme>> Retrieve(ILogger logger, CancellationToken cancellationToken) =>
        Retrieve("*", logger, cancellationToken);

    public Task<IReadOnlyCollection<LocalScheme>> Retrieve(
        string pattern,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var gameInfo = wormsLocator.Find();

        if (!gameInfo.IsInstalled)
        {
            return Task.FromResult<IReadOnlyCollection<LocalScheme>>(new List<LocalScheme>(0));
        }

        var schemes = new List<LocalScheme>();

        foreach (var scheme in fileSystem.Directory.GetFiles(gameInfo.SchemesFolder, $"{pattern}.wsc"))
        {
            var fileName = fileSystem.Path.GetFileNameWithoutExtension(scheme);
            var details = wscReader.Read(scheme);
            schemes.Add(new LocalScheme(scheme, fileName, details));
        }

        return Task.FromResult<IReadOnlyCollection<LocalScheme>>(schemes);
    }
}

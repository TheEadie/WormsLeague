using System.IO.Abstractions;
using Serilog;
using Worms.Armageddon.Game;
using Worms.Armageddon.Files.Schemes.Binary;

namespace Worms.Cli.Resources.Local.Schemes;

internal sealed class LocalSchemesRetriever : IResourceRetriever<LocalScheme>
{
    private readonly IWscReader _wscReader;
    private readonly IWormsLocator _wormsLocator;
    private readonly IFileSystem _fileSystem;

    public LocalSchemesRetriever(IWscReader wscReader, IWormsLocator wormsLocator, IFileSystem fileSystem)
    {
        _wscReader = wscReader;
        _wormsLocator = wormsLocator;
        _fileSystem = fileSystem;
    }

    public Task<IReadOnlyCollection<LocalScheme>> Retrieve(ILogger logger, CancellationToken cancellationToken)
        => Retrieve("*", logger, cancellationToken);

    public Task<IReadOnlyCollection<LocalScheme>> Retrieve(string pattern, ILogger logger, CancellationToken cancellationToken)
    {
        var gameInfo = _wormsLocator.Find();

        if (!gameInfo.IsInstalled)
        {
            return Task.FromResult<IReadOnlyCollection<LocalScheme>>(new List<LocalScheme>(0));
        }

        var schemes = new List<LocalScheme>();

        foreach (var scheme in _fileSystem.Directory.GetFiles(gameInfo.SchemesFolder, $"{pattern}.wsc"))
        {
            var fileName = _fileSystem.Path.GetFileNameWithoutExtension(scheme);
            var details = _wscReader.Read(scheme);
            schemes.Add(new LocalScheme(scheme, fileName, details));
        }

        return Task.FromResult<IReadOnlyCollection<LocalScheme>>(schemes);
    }
}

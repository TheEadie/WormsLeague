using System.IO.Abstractions;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Game;

namespace Worms.Cli.Resources.Local.Schemes;

internal sealed class LocalSchemesRetriever(
    IWscReader wscReader,
    IWormsArmageddon wormsArmageddon,
    IFileSystem fileSystem) : IResourceRetriever<LocalScheme>
{
    public Task<IReadOnlyCollection<LocalScheme>> Retrieve(CancellationToken cancellationToken) =>
        Retrieve("*", cancellationToken);

    public Task<IReadOnlyCollection<LocalScheme>> Retrieve(string pattern, CancellationToken cancellationToken)
    {
        var gameInfo = wormsArmageddon.FindInstallation();

        if (!gameInfo.IsInstalled)
        {
            return Task.FromResult<IReadOnlyCollection<LocalScheme>>([]);
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

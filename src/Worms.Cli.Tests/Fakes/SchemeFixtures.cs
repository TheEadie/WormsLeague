namespace Worms.Cli.Tests.Fakes;

internal static class SchemeFixtures
{
    private static readonly Lazy<byte[]> RedgateBytes = new(() =>
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "sample-data", "schemes", "redgate.wsc");
        return File.ReadAllBytes(path);
    });

    /// <summary>
    /// Writes the bytes of sample-data/schemes/redgate.wsc into MockFileSystem at
    /// &lt;SchemesFolder&gt;/&lt;schemeName&gt;.wsc so LocalSchemesRetriever can discover it.
    /// </summary>
    public static void WriteScheme(TestHost host, string schemeName)
    {
        var info = host.WormsArmageddon.FindInstallation();
        var fs = host.FileSystem;
        var path = fs.Path.Combine(info.SchemesFolder, schemeName + ".wsc");
        fs.File.WriteAllBytes(path, RedgateBytes.Value);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docker-compose.yaml")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not find repo root (docker-compose.yaml not found)");
    }
}

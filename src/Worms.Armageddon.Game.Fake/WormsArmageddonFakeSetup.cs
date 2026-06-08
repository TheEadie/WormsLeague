using System.IO.Abstractions;
using System.Reflection;

namespace Worms.Armageddon.Game.Fake;

/// <summary>
/// Test-setup helper for the installed fake. Seeds the fake's on-disk layout so the code under test can discover
/// replays and schemes. Kept off <see cref="IWormsArmageddon"/> as it is not part of the production contract, and
/// injected with the same <see cref="IFileSystem"/> the fake uses so seeded files land where the fake reports its
/// installation.
/// </summary>
public sealed class WormsArmageddonFakeSetup(IFileSystem fileSystem, IWormsArmageddon wormsArmageddon)
{
    /// <summary>
    /// A replay log spanning two teams over multiple turns, for tests that parse replay details.
    /// </summary>
    public const string MultiTurnReplayLog = """
                                             Game Started at 2024-01-02 10:00:00 GMT
                                             Red: "a person" as "Some Team"
                                             Blue: "another person" as "Team 2"
                                             [00:06:59.08] ••• Some Team (a person) starts turn
                                             [00:07:08.26] ••• Some Team (a person) fires Shotgun
                                             [00:07:26.60] ••• Some Team (a person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat
                                             [00:09:59.08] ••• Team 2 (another person) starts turn
                                             [00:10:08.26] ••• Team 2 (another person) fires Shotgun
                                             [00:11:26.60] ••• Team 3 (another person) ends turn; time used: 11.58 sec turn, 3.00 sec retreat
                                             """;

    private static readonly Lazy<byte[]> SampleScheme = new(LoadSampleScheme);

    /// <summary>
    /// Writes an empty replay file (and optional .log) into the fake's replay folder so the CLI can discover it.
    /// </summary>
    public void WriteReplay(string filenameNoExt, string? logContent = null)
    {
        var info = wormsArmageddon.FindInstallation();
        fileSystem.File.WriteAllBytes(fileSystem.Path.Combine(info.ReplayFolder, filenameNoExt + ".WAgame"), []);
        if (logContent is not null)
        {
            fileSystem.File.WriteAllText(
                fileSystem.Path.Combine(info.ReplayFolder, filenameNoExt + ".log"),
                logContent);
        }
    }

    /// <summary>
    /// Writes a real sample scheme into the fake's schemes folder at &lt;SchemesFolder&gt;/&lt;schemeName&gt;.wsc so
    /// LocalSchemesRetriever can discover and parse it.
    /// </summary>
    public void WriteScheme(string schemeName)
    {
        var info = wormsArmageddon.FindInstallation();
        var path = fileSystem.Path.Combine(info.SchemesFolder, schemeName + ".wsc");
        fileSystem.File.WriteAllBytes(path, SampleScheme.Value);
    }

    private static byte[] LoadSampleScheme()
    {
        const string resourceName = "Worms.Armageddon.Game.Fake.redgate.wsc";
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded scheme resource '{resourceName}' was not found.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}

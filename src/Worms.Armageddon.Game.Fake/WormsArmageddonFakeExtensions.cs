using System.Reflection;

namespace Worms.Armageddon.Game.Fake;

/// <summary>
/// Test-setup helpers for the installed fake. These seed the fake's on-disk layout so the code under test can
/// discover replays and schemes; they are deliberately kept off <see cref="IWormsArmageddon"/> as they are not
/// part of the production contract.
/// </summary>
public static class WormsArmageddonFakeExtensions
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
    public static void WriteReplay(this IWormsArmageddon wormsArmageddon, string filenameNoExt, string? logContent = null)
    {
        var fake = AsInstalled(wormsArmageddon);
        var info = fake.FindInstallation();
        fake.FileSystem.File.WriteAllBytes(
            fake.FileSystem.Path.Combine(info.ReplayFolder, filenameNoExt + ".WAgame"), []);
        if (logContent is not null)
        {
            fake.FileSystem.File.WriteAllText(
                fake.FileSystem.Path.Combine(info.ReplayFolder, filenameNoExt + ".log"), logContent);
        }
    }

    /// <summary>
    /// Writes a real sample scheme into the fake's schemes folder at &lt;SchemesFolder&gt;/&lt;schemeName&gt;.wsc so
    /// LocalSchemesRetriever can discover and parse it.
    /// </summary>
    public static void WriteScheme(this IWormsArmageddon wormsArmageddon, string schemeName)
    {
        var fake = AsInstalled(wormsArmageddon);
        var info = fake.FindInstallation();
        var path = fake.FileSystem.Path.Combine(info.SchemesFolder, schemeName + ".wsc");
        fake.FileSystem.File.WriteAllBytes(path, SampleScheme.Value);
    }

    private static Installed AsInstalled(IWormsArmageddon wormsArmageddon) =>
        wormsArmageddon as Installed
        ?? throw new InvalidOperationException("Fake setup helpers only apply to the installed Worms Armageddon fake.");

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

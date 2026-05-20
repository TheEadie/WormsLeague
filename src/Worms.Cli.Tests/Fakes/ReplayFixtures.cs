namespace Worms.Cli.Tests.Fakes;

internal static class ReplayFixtures
{
    public const string MultiTurnLog = """
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

    public static void WriteReplay(
        TestHost host,
        string filenameNoExt,
        string? logContent = null)
    {
        var info = host.WormsArmageddon.FindInstallation();
        var fs = host.FileSystem;
        fs.File.WriteAllBytes(fs.Path.Combine(info.ReplayFolder, filenameNoExt + ".WAgame"), []);
        if (logContent is not null)
        {
            fs.File.WriteAllText(fs.Path.Combine(info.ReplayFolder, filenameNoExt + ".log"), logContent);
        }
    }
}

namespace Worms.Armageddon.Files.Replays.Filename;

public record ReplayFilenameInfo(
    DateTime Date,
    string GameMode,
    IReadOnlyCollection<string>? PlayerMachineNames,
    string? HostMachineName,
    string? LocalMachineName)
{
    public static ReplayFilenameInfo Online(
        DateTime date,
        IReadOnlyCollection<string> playerMachineNames,
        string hostMachineName,
        string localMachineName) =>
        new(date, "Online", playerMachineNames, hostMachineName, localMachineName);

    public static ReplayFilenameInfo Other(DateTime date, string gameMode) => new(date, gameMode, null, null, null);
}

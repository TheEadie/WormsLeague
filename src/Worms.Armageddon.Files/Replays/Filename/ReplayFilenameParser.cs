namespace Worms.Armageddon.Files.Replays.Filename;

internal sealed class ReplayFilenameParser : IReplayFilenameParser
{
    public ReplayFilenameInfo Parse(string filename)
    {
        var fileName = Path.GetFileNameWithoutExtension(filename);

        var startIndexForOnlineBlock = fileName.IndexOf('[', StringComparison.InvariantCulture);
        var endIndexForOnlineBlock = fileName.IndexOf(']', StringComparison.InvariantCulture);

        var dateString = fileName[..(startIndexForOnlineBlock - 1)];
        var date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH.mm.ss", null);

        var gameModeString = fileName[(startIndexForOnlineBlock + 1)..endIndexForOnlineBlock];

        if (gameModeString != "Online")
        {
            return ReplayFilenameInfo.Other(date, gameModeString);
        }

        var playerNamesString = fileName[(endIndexForOnlineBlock + 2)..];
        var playerNames = playerNamesString.Split(", ", StringSplitOptions.RemoveEmptyEntries);
        var hostPlayerName = playerNames[0];
        var localPlayerName = playerNames.First(x => x.StartsWith('@'));

        return ReplayFilenameInfo.Online(
            date,
            [.. playerNames.Select(RemoveAtSymbol)],
            RemoveAtSymbol(hostPlayerName),
            RemoveAtSymbol(localPlayerName));
    }

    private static string RemoveAtSymbol(string playerName) =>
        playerName.StartsWith('@') ? playerName[1..] : playerName;
}

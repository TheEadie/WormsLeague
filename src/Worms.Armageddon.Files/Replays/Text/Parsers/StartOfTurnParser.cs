using System.Globalization;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Files.Replays.Text.Parsers;

internal sealed class StartOfTurnParser : IReplayLineParser
{
    private const string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";
    private const string TeamName = "(.+)";
    private static readonly Regex StartOfTurn = new($"{Timestamp} (•••|���) {TeamName} starts turn");

    public bool CanParse(string line) => StartOfTurn.IsMatch(line);

    public void Parse(string line, ReplayResourceBuilder builder)
    {
        var startOfTurn = StartOfTurn.Match(line);

        if (startOfTurn.Success)
        {
            var startTimeTurn = TimeSpan.Parse(startOfTurn.Groups[1].Value, CultureInfo.CurrentCulture);
            var teamName = GetTeamNameFromText(startOfTurn.Groups[3].Value);

            _ = builder.FinaliseCurrentTurn();
            _ = builder.CurrentTurn.WithStartTime(startTimeTurn).WithTeam(builder.GetTeamByName(teamName));
        }
    }

    private static string GetTeamNameFromText(string text) => text.Split('(')[0].Trim();
}

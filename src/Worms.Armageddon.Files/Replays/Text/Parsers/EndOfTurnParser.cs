using System.Globalization;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Files.Replays.Text.Parsers;

internal sealed partial class EndOfTurnParser : IReplayLineParser
{
    private const string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";
    private const string TeamName = "(.+)";

    [GeneratedRegex($"{Timestamp} (•••|���) {TeamName} .*; time used:.*sec")]
    private static partial Regex EndOfTurn();

    public bool CanParse(string line) => EndOfTurn().IsMatch(line);

    public void Parse(string line, ReplayResourceBuilder builder)
    {
        var endOfTurn = EndOfTurn().Match(line);

        if (endOfTurn.Success)
        {
            _ = builder.CurrentTurn.WithEndTime(TimeSpan.Parse(endOfTurn.Groups[1].Value, CultureInfo.CurrentCulture));
        }
    }
}

using System.Globalization;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Files.Replays.Text.Parsers;

internal sealed class StartTimeParser : IReplayLineParser
{
    private const string DateAndTime = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})";
    private static readonly Regex StartTime = new($"Game Started at {DateAndTime} GMT");

    public bool CanParse(string line) => StartTime.IsMatch(line);

    public void Parse(string line, ReplayResourceBuilder builder)
    {
        var match = StartTime.Match(line);
        if (match.Success)
        {
            _ = builder.WithStartTime(DateTime.Parse(match.Groups[1].Value, CultureInfo.CurrentCulture));
        }
    }
}

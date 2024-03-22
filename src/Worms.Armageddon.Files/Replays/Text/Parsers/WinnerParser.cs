using System.Text.RegularExpressions;

namespace Worms.Armageddon.Files.Replays.Text.Parsers;

internal sealed class WinnerParser : IReplayLineParser
{
    private const string TeamName = "(.+)";
    private static readonly Regex WinnerDraw = new("The round was drawn.");
    private static readonly Regex Winner = new($"{TeamName} wins the (match!|round.)");

    public bool CanParse(string line) => WinnerDraw.IsMatch(line) || Winner.IsMatch(line);

    public void Parse(string line, ReplayResourceBuilder builder)
    {
        var winnerDrawMatch = WinnerDraw.Match(line);
        var winnerMatch = Winner.Match(line);

        if (winnerDrawMatch.Success)
        {
            _ = builder.WithWinner("Draw");
        }
        else if (winnerMatch.Success)
        {
            _ = builder.WithWinner(winnerMatch.Groups[1].Value);
        }
    }
}

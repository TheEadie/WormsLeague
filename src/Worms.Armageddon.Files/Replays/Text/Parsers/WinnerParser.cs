using System.Text.RegularExpressions;

namespace Worms.Armageddon.Files.Replays.Text.Parsers;

internal sealed partial class WinnerParser : IReplayLineParser
{
    private const string TeamName = "(.+)";

    [GeneratedRegex("The round was drawn.")]
    private static partial Regex WinnerDraw();

    [GeneratedRegex($"{TeamName} wins the (match!|round.)")]
    private static partial Regex Winner();

    public bool CanParse(string line) => WinnerDraw().IsMatch(line) || Winner().IsMatch(line);

    public void Parse(string line, ReplayResourceBuilder builder)
    {
        var winnerDrawMatch = WinnerDraw().Match(line);
        var winnerMatch = Winner().Match(line);

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

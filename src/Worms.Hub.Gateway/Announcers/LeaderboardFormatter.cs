using System.Globalization;
using System.Text;

namespace Worms.Hub.Gateway.Announcers;

internal static class LeaderboardFormatter
{
    public static string Format(IReadOnlyList<LeaderboardEntry> entries)
    {
        var rankWidth = entries.Max(e => e.Rank.ToString(CultureInfo.InvariantCulture).Length);
        var eloWidth = entries.Max(e => e.Elo.ToString(CultureInfo.InvariantCulture).Length);

        var sb = new StringBuilder();
        sb.AppendLine("Leaderboard:");

        foreach (var entry in entries)
        {
            var rank = entry.Rank.ToString(CultureInfo.InvariantCulture).PadLeft(rankWidth);
            var elo = entry.Elo.ToString(CultureInfo.InvariantCulture).PadLeft(eloWidth);

            var safeName = entry.DisplayName
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal);

            var suffix = BuildSuffix(entry);

            sb.AppendLine(CultureInfo.InvariantCulture, $"{rank}: {elo} {safeName}{suffix}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string BuildSuffix(LeaderboardEntry entry)
    {
        string delta;
        if (entry.EloDelta is { } d and not 0)
        {
            delta = d > 0
                ? $" (+{d.ToString(CultureInfo.InvariantCulture)})"
                : $" ({d.ToString(CultureInfo.InvariantCulture)})";
        }
        else
        {
            delta = string.Empty;
        }

        string arrow;
        if (entry.PositionChange is { } c)
        {
            arrow = c < 0
                ? $" ⇧{(-c).ToString(CultureInfo.InvariantCulture)}"
                : $" ⇩{c.ToString(CultureInfo.InvariantCulture)}";
        }
        else
        {
            arrow = string.Empty;
        }

        return delta + arrow;
    }
}

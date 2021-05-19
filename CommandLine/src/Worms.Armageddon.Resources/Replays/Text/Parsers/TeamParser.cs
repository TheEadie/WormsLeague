using System;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Resources.Replays.Text.Parsers
{
    internal class TeamParser : IReplayLineParser
    {
        private const string TeamColour = @"(Red|Blue|Green|Yellow|Magenta|Cyan)";
        private const string TeamName = @"(.+)";
        private const string PlayerName = @"(.+)";

        private static readonly Regex TeamSummaryOnline = new Regex($"{TeamColour}:.+\"{PlayerName}\".+as.+\"{TeamName}\"");
        private static readonly Regex TeamSummaryOffline = new Regex($"{TeamColour}:.+\"{TeamName}\"");

        public bool CanParse(string line) => TeamSummaryOnline.IsMatch(line) || TeamSummaryOffline.IsMatch(line);

        public void Parse(string line, ReplayResourceBuilder builder)
        {
            var teamSummaryOnlineMatch = TeamSummaryOnline.Match(line);
            var teamSummaryOfflineMatch = TeamSummaryOffline.Match(line);

            if (teamSummaryOnlineMatch.Success)
            {
                builder.WithTeam(Team.Remote(
                    teamSummaryOnlineMatch.Groups[3].Value,
                    teamSummaryOnlineMatch.Groups[2].Value,
                    Enum.Parse<TeamColour>(teamSummaryOnlineMatch.Groups[1].Value)));
            }
            else if (teamSummaryOfflineMatch.Success)
            {
                builder.WithTeam(Team.Local(
                    teamSummaryOfflineMatch.Groups[2].Value,
                    Enum.Parse<TeamColour>(teamSummaryOfflineMatch.Groups[1].Value)));
            }
        }
    }
}

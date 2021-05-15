using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Resources.Replays.Text
{
    internal class ReplayTextReader : IReplayTextReader
    {
        private const string DateAndTime = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})";

        private const string TeamColour = @"(Red|Blue|Green|Yellow|Magenta|Cyan)";
        private const string TeamName = @"(.+)";
        private const string PlayerName = @"(.+)";

        private static readonly Regex StartTime = new Regex($"Game Started at {DateAndTime} GMT");
        private static readonly Regex TeamSummaryOnline = new Regex($"{TeamColour}:.*\"{PlayerName}\" as .*\"{TeamName}\"");
        private static readonly Regex TeamSummaryOffline = new Regex($"{TeamColour}:.*\"{TeamName}\"");

        private static readonly Regex WinnerDraw = new Regex($"The round was drawn.");
        private static readonly Regex Winner = new Regex($"{TeamName} wins the (match!|round.)");

        public ReplayResource GetModel(string definition)
        {
            var startTime = DateTime.MinValue;
            var teams = new List<Team>();
            var winner = string.Empty;

            foreach (var line in definition.Split('\n'))
            {
                var startTimeMatch = StartTime.Match(line);
                var teamSummaryOnlineMatch = TeamSummaryOnline.Match(line);
                var teamSummaryOfflineMatch = TeamSummaryOffline.Match(line);

                var winnerDrawMatch = WinnerDraw.Match(line);
                var winnerMatch = Winner.Match(line);

                if (startTimeMatch.Success)
                {
                    startTime = DateTime.Parse(startTimeMatch.Groups[1].Value);
                }

                if (teamSummaryOnlineMatch.Success)
                {
                    teams.Add(Team.Remote(
                        teamSummaryOnlineMatch.Groups[3].Value,
                        teamSummaryOnlineMatch.Groups[2].Value,
                        Enum.Parse<TeamColour>(teamSummaryOnlineMatch.Groups[1].Value)));
                }
                else if (teamSummaryOfflineMatch.Success)
                {
                    teams.Add(Team.Local(
                        teamSummaryOfflineMatch.Groups[2].Value,
                        Enum.Parse<TeamColour>(teamSummaryOfflineMatch.Groups[1].Value)));
                }

                if (winnerDrawMatch.Success)
                {
                    winner = "Draw";
                }
                else if (winnerMatch.Success)
                {
                    winner = winnerMatch.Groups[1].Value;
                }
            }

            return new ReplayResource(
                startTime,
                "local",
                true,
                teams,
                winner,
                new List<Turn>(),
                definition);
        }
    }
}

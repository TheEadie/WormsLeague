using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Worms.Resources.Games.Text
{
    public class GameTextReader : IGameTextReader
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

        public GameResource GetModel(string definition)
        {
            var startTime = DateTime.MinValue;
            var teams = new List<string>();
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
                    teams.Add(teamSummaryOnlineMatch.Groups[3].Value);
                }
                else if (teamSummaryOfflineMatch.Success)
                {
                    teams.Add(teamSummaryOfflineMatch.Groups[2].Value);
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

            return new GameResource(startTime, "local", true, teams, winner, definition);
        }
    }
}

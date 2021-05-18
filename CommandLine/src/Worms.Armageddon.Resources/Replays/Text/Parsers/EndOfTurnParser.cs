using System;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Resources.Replays.Text.Parsers
{
    internal class EndOfTurnParser : IReplayLineParser
    {
        private const string Timestamp = @"\[(\d+:\d+:\d+.\d+)\]";
        private const string TeamName = @"(.+)";
        private static readonly Regex EndOfTurn = new($"{Timestamp} ••• {TeamName} .*; time used:.*sec");

        public bool CanParse(string line) => EndOfTurn.IsMatch(line);

        public void Parse(string line, ReplayResourceBuilder builder)
        {
            var endOfTurn = EndOfTurn.Match(line);

            if (endOfTurn.Success)
            {
                builder.CurrentTurn.WithEndTime(TimeSpan.Parse(endOfTurn.Groups[1].Value));
            }
        }
    }
}

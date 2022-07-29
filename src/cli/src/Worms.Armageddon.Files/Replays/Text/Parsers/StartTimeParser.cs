using System;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Resources.Replays.Text.Parsers
{
    internal class StartTimeParser : IReplayLineParser
    {
        private const string DateAndTime = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})";
        private static readonly Regex StartTime = new Regex($"Game Started at {DateAndTime} GMT");

        public bool CanParse(string line) => StartTime.IsMatch(line);

        public void Parse(string line, ReplayResourceBuilder builder)
        {
            var match = StartTime.Match(line);
            if (match.Success)
            {
                builder.WithStartTime(DateTime.Parse(match.Groups[1].Value));
            }
        }
    }
}

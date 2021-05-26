using System.Collections.Generic;
using System.Linq;

namespace Worms.Armageddon.Resources.Replays.Text
{
    internal class ReplayTextReader : IReplayTextReader
    {
        private readonly IEnumerable<IReplayLineParser> _parsers;

        public ReplayTextReader(IEnumerable<IReplayLineParser> parsers)
        {
            _parsers = parsers;
        }

        public ReplayResource GetModel(string definition)
        {
            var builder = new ReplayResourceBuilder();

            foreach (var line in definition.Split('\n'))
            {
                var matchingParsers = _parsers.Where(x => x.CanParse(line));
                matchingParsers.ToList().ForEach(x => x.Parse(line, builder));
            }

            return builder
                .FinaliseCurrentTurn()
                .WithFullLog(definition)
                .Build();
        }
    }
}

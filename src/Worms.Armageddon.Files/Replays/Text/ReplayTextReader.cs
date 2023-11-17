namespace Worms.Armageddon.Files.Replays.Text;

internal class ReplayTextReader(IEnumerable<IReplayLineParser> parsers) : IReplayTextReader
{
    public ReplayResource GetModel(string definition)
    {
        var builder = new ReplayResourceBuilder();

        foreach (var line in definition.Split('\n'))
        {
            var matchingParsers = parsers.Where(x => x.CanParse(line));
            matchingParsers.ToList().ForEach(x => x.Parse(line, builder));
        }

        return builder.FinaliseCurrentTurn().WithFullLog(definition).Build();
    }
}

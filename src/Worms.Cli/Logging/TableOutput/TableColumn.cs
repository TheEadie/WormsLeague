namespace Worms.Cli.Logging.TableOutput;

internal sealed class TableColumn(string heading, IReadOnlyCollection<string> rows, int width)
{
    public string Heading { get; } = heading;
    public IReadOnlyCollection<string> Rows { get; } = rows;
    public int Width { get; } = width;
}

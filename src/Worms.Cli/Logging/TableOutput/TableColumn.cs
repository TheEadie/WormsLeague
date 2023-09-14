namespace Worms.Cli.Logging.TableOutput;

internal class TableColumn
{
    public string Heading { get; }
    public IReadOnlyCollection<string> Rows { get; }
    public int Width { get; }

    public TableColumn(string heading, IReadOnlyCollection<string> rows, int width)
    {
        Heading = heading;
        Rows = rows;
        Width = width;
    }
}

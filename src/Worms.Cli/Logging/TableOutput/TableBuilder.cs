namespace Worms.Cli.Logging.TableOutput;

internal sealed class TableBuilder(int outputWidth)
{
    private readonly List<TableColumn> _columns = [];
    private const int ColumnPadding = 3;

    public void AddColumn(string heading, IReadOnlyCollection<string> rows) =>
        _columns.Add(new TableColumn(heading, rows, GetWidth(heading, rows)));

    public Table Build()
    {
        // If there's no columns return early
        if (_columns.Count == 0)
        {
            return new Table(_columns, 0);
        }

        // Add columns for the amount of screen space
        var adjustedColumns = new List<TableColumn>();
        var currentWidth = 0;

        foreach (var column in _columns)
        {
            // Only add another column if the heading can be rendered
            var headingLength = column.Heading.Length + ColumnPadding;
            if (currentWidth >= outputWidth - headingLength)
            {
                break;
            }

            adjustedColumns.Add(column);
            currentWidth += column.Width;
        }

        // Adjust the last column so it doesn't wrap the line
        var lastColumn = adjustedColumns[^1];
        _ = adjustedColumns.Remove(lastColumn);
        var remainingWidth = outputWidth - adjustedColumns.Sum(x => x.Width) - 1;

        adjustedColumns.Add(
            new TableColumn(
                TrimText(lastColumn.Heading, remainingWidth),
                [.. lastColumn.Rows.Select(x => TrimText(x, remainingWidth))],
                remainingWidth));

        return new Table(adjustedColumns, adjustedColumns.Max(x => x.Rows.Count));
    }

    private static string TrimText(string input, int maxLength)
    {
        const string truncate = "..";
        var truncatedString = input;

        if (input.Length > maxLength && maxLength > truncate.Length)
        {
            truncatedString = input[..(maxLength - truncate.Length)] + truncate;
        }

        if (input.Length > maxLength && maxLength <= truncate.Length)
        {
            truncatedString = input[..maxLength];
        }

        return truncatedString;
    }

    private static int GetWidth(string header, IReadOnlyCollection<string> values)
    {
        var anyItems = values.Count != 0;
        var headerLength = header.Length + ColumnPadding;
        var longest = anyItems ? values.Max(x => x.Length) + ColumnPadding : headerLength;
        if (longest < headerLength)
        {
            longest = headerLength;
        }

        return longest;
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Worms.Logging.TableOutput
{
    public class TableBuilder
    {
        private readonly int _outputWidth;
        private readonly List<TableColumn> _columns = new List<TableColumn>();

        public TableBuilder(int outputWidth)
        {
            _outputWidth = outputWidth;
        }

        public void AddColumn(string heading, IReadOnlyCollection<string> rows)
        {
            _columns.Add(new TableColumn(heading, rows, GetWidth(heading, rows)));
        }

        public Table Build()
        {
            if (!_columns.Any())
            {
                return new Table(_columns, 0);
            }

            var adjustedColumns = new List<TableColumn>();
            var currentWidth = 0;

            foreach (var column in _columns.TakeWhile(column => currentWidth <= _outputWidth))
            {
                adjustedColumns.Add(column);
                currentWidth += column.Width;
            }

            var lastColumn = adjustedColumns.Last();

            adjustedColumns.Remove(lastColumn);
            var remainingWidth = _outputWidth - adjustedColumns.Sum(x => x.Width) - 1;

            adjustedColumns.Add(
                new TableColumn(
                    TrimText(lastColumn.Heading, remainingWidth),
                    lastColumn.Rows.Select(x => TrimText(x, remainingWidth)).ToList(),
                    remainingWidth));

            return new Table(adjustedColumns, adjustedColumns.Max(x => x.Rows.Count));
        }

        private static string TrimText(string input, int maxLength)
        {
            return input.Length <= maxLength ? input : input.Substring(0, maxLength - 4) + " ...";
        }

        private static int GetWidth(string header, IReadOnlyCollection<string> values)
        {
            var anyItems = values.Any();
            var headerLength = header.Length + 3;
            var longest = anyItems ? values.Max(x => x.Length) + 3 : headerLength;
            if (longest < headerLength)
                longest = headerLength;
            return longest;
        }
    }
}

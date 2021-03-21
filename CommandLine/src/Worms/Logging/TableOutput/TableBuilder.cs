﻿using System.Collections.Generic;
using System.Linq;

namespace Worms.Logging.TableOutput
{
    public class TableBuilder
    {
        private readonly int _outputWidth;
        private readonly List<TableColumn> _columns = new List<TableColumn>();
        private const int _columnPadding = 3;

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
            // If there's no columns return early
            if (!_columns.Any())
            {
                return new Table(_columns, 0);
            }

            // Add columns for the amount of screen space
            var adjustedColumns = new List<TableColumn>();
            var currentWidth = 0;

            foreach (var column in _columns)
            {
                // Only add another column if the heading can be rendered
                var headingLength = column.Heading.Length + _columnPadding;
                if (currentWidth >= _outputWidth - headingLength)
                    break;

                adjustedColumns.Add(column);
                currentWidth += column.Width;
            }

            // Adjust the last column so it doesn't wrap the line
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
            const string truncate = "..";

            if (maxLength <= truncate.Length)
            {
                return input.Length <= maxLength ? input : input.Substring(0, maxLength);
            }

            return input.Length <= maxLength ? input : input.Substring(0, maxLength - truncate.Length) + truncate;
        }

        private static int GetWidth(string header, IReadOnlyCollection<string> values)
        {
            var anyItems = values.Any();
            var headerLength = header.Length + _columnPadding;
            var longest = anyItems ? values.Max(x => x.Length) + _columnPadding : headerLength;
            if (longest < headerLength)
                longest = headerLength;
            return longest;
        }
    }
}

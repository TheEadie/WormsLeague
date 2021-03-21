using System.Collections.Generic;

namespace Worms.Logging.TableOutput
{
    public class Table
    {
        internal IReadOnlyCollection<TableColumn> Columns { get; }
        internal int RowCount { get; }

        internal Table(IReadOnlyCollection<TableColumn> columns, int rowCount)
        {
            Columns = columns;
            RowCount = rowCount;
        }
    }
}

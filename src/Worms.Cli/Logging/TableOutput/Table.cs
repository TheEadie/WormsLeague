using System.Collections.Generic;

namespace Worms.Logging.TableOutput
{
    internal record Table(IReadOnlyCollection<TableColumn> Columns, int RowCount);
}

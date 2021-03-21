using System.IO;
using System.Linq;

namespace Worms.Logging.TableOutput
{
    public static class TablePrinter
    {
        public static void Print(TextWriter writer, Table table)
        {
            foreach (var column in table.Columns)
            {
                writer.Write(column.Heading.PadRight(column.Width));
            }

            writer.WriteLine();

            for (var i = 0; i < table.RowCount; i++)
            {
                foreach (var column in table.Columns)
                {
                    writer.Write(column.Rows.ElementAt(i).PadRight(column.Width));
                }

                writer.WriteLine();
            }
        }
    }
}

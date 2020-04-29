using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using Worms.Resources.Schemes;

namespace Worms.Logging
{
    internal class TablePrinter
    {
        public void Print(TextWriter writer, IReadOnlyCollection<SchemeResource> items)
        {
            var anyResourcesToPrint = items.Any();
            var longestName = anyResourcesToPrint ? items.Max(x => x.Name.Length) + 3 : 7;
            var longestContext = anyResourcesToPrint ? items.Max(x => x.Context.Length) + 3 : 10;

            writer.WriteLine("NAME".PadRight(longestName) + "CONTEXT".PadRight(longestContext));

            foreach(var item in items)
            {
                writer.WriteLine(item.Name.PadRight(longestName) + item.Context.PadRight(longestContext));
            }
        }
    }
}

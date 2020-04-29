using System.Collections.Generic;
using System.Linq;
using Serilog;
using Worms.Resources.Schemes;

namespace Worms.Logging
{
    internal class TablePrinter
    {
        public void Print(ILogger logger, IEnumerable<SchemeResource> items)
        {
            bool anyResourcesToPrint = items.Any();
            var longestName = anyResourcesToPrint ? items.Max(x => x.Name.Length) + 3 : 7;
            var longestContext = anyResourcesToPrint ? items.Max(x => x.Context.Length) + 3 : 10;

            logger.Information("NAME".PadRight(longestName) + "CONTEXT".PadRight(longestContext));

            foreach(var item in items)
            {
                logger.Information(SeriiLogEscape(item.Name.PadRight(longestName)) + item.Context.PadRight(longestContext));
            }
        }

        private string SeriiLogEscape(string input)
        {
            // Special case for text like {{01}} which appears in the default schipped worms scheme names
            // This needs a more general fix to tell seriilog not to treat anything as a special char
            return input.Replace("{{", "{{{").Replace("}}", "}}}");
        }
    }
}
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Worms.Logging.TableOutput;

namespace Worms.Resources.Games.Text
{
    public class GameTextPrinter : IResourcePrinter<GameResource>
    {
        public void Print(TextWriter writer, IReadOnlyCollection<GameResource> items, int outputMaxWidth)
        {
            var tableBuilder = new TableBuilder(outputMaxWidth);

            tableBuilder.AddColumn("DATE", items.Select(x => x.Date.ToString(CultureInfo.CurrentCulture)).ToList());
            tableBuilder.AddColumn("CONTEXT", items.Select(x => x.Context).ToList());
            tableBuilder.AddColumn("PROCESSED", items.Select(x => x.Processed.ToString()).ToList());
            tableBuilder.AddColumn("WINNER", items.Select(x => x.Winner.ToString()).ToList());
            tableBuilder.AddColumn("TEAMS", items.Select(x => string.Join(", ", x.Teams)).ToList());

            var table = tableBuilder.Build();
            TablePrinter.Print(writer, table);
        }

        public void Print(TextWriter writer, GameResource resource, int outputMaxWidth)
        {
            Print(writer, new List<GameResource> {resource}, outputMaxWidth);
        }
    }
}

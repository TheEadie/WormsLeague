using System.Collections.Generic;
using System.IO;
using System.Linq;
using Worms.Logging;
using Worms.Logging.TableOutput;

namespace Worms.Resources.Games.Text
{
    public class GameTextPrinter : IResourcePrinter<GameResource>
    {
        public void Print(TextWriter writer, IReadOnlyCollection<GameResource> items, int outputMaxWidth)
        {
            var tableBuilder = new TableBuilder(outputMaxWidth);

            tableBuilder.AddColumn("NAME", items.Select(x => x.Name).ToList());
            tableBuilder.AddColumn("CONTEXT", items.Select(x => x.Context).ToList());

            var table = tableBuilder.Build();
            TablePrinter.Print(writer, table);
        }

        public void Print(TextWriter writer, GameResource resource, int outputMaxWidth)
        {
            Print(writer, new List<GameResource>{resource}, outputMaxWidth);
        }
    }
}

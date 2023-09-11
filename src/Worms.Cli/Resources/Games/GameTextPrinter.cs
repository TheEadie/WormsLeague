using System.Collections.Generic;
using System.IO;
using System.Linq;
using Worms.Armageddon.Resources.Schemes.Text;
using Worms.Cli.Logging.TableOutput;
using Worms.Cli.Resources.Remote.Games;

namespace Worms.Cli.Resources.Games
{
    internal class GameTextPrinter : IResourcePrinter<RemoteGame>
    {
        private readonly ISchemeTextWriter _schemeTextWriter;

        public GameTextPrinter(ISchemeTextWriter schemeTextWriter)
        {
            _schemeTextWriter = schemeTextWriter;
        }

        public void Print(TextWriter writer, IReadOnlyCollection<RemoteGame> items, int outputMaxWidth)
        {
            var tableBuilder = new TableBuilder(outputMaxWidth);
            tableBuilder.AddColumn("ID", items.Select(x => x.Id).ToList());
            tableBuilder.AddColumn("HOST", items.Select(x => x.HostMachine).ToList());
            tableBuilder.AddColumn("STATUS", items.Select(x => x.Status).ToList());

            var table = tableBuilder.Build();
            TablePrinter.Print(writer, table);
        }

        public void Print(TextWriter writer, RemoteGame resource, int outputMaxWidth)
        {
            writer.WriteLine($"To join this game visit wa://{resource.HostMachine}");
        }
    }
}

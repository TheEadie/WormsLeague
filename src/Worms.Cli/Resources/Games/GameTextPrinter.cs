using Worms.Cli.Logging.TableOutput;
using Worms.Cli.Resources.Remote.Games;

namespace Worms.Cli.Resources.Games;

internal sealed class GameTextPrinter : IResourcePrinter<RemoteGame>
{
    public void Print(TextWriter writer, IReadOnlyCollection<RemoteGame> resources, int outputMaxWidth)
    {
        var tableBuilder = new TableBuilder(outputMaxWidth);
        tableBuilder.AddColumn("ID", resources.Select(x => x.Id).ToList());
        tableBuilder.AddColumn("HOST", resources.Select(x => x.HostMachine).ToList());
        tableBuilder.AddColumn("STATUS", resources.Select(x => x.Status).ToList());

        var table = tableBuilder.Build();
        TablePrinter.Print(writer, table);
    }

    public void Print(TextWriter writer, RemoteGame resource, int outputMaxWidth) =>
        writer.WriteLine($"To join this game visit wa://{resource.HostMachine}");
}

using System.Globalization;
using Worms.Armageddon.Files.Replays;
using Worms.Cli.Logging.TableOutput;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Resources.Replays;

internal sealed class ReplayTextPrinter : IResourcePrinter<LocalReplay>
{
    public void Print(TextWriter writer, IReadOnlyCollection<LocalReplay> resources, int outputMaxWidth)
    {
        var tableBuilder = new TableBuilder(outputMaxWidth);

        tableBuilder.AddColumn(
            "NAME",
            [.. resources.Select(x => x.Details.Date.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture))]);
        tableBuilder.AddColumn("CONTEXT", [.. resources.Select(x => x.Context)]);
        tableBuilder.AddColumn("PROCESSED", [.. resources.Select(x => x.Details.Processed.ToString())]);
        tableBuilder.AddColumn("WINNER", [.. resources.Select(x => x.Details.Winner)]);
        tableBuilder.AddColumn(
            "TEAMS",
            [.. resources.Select(x => string.Join(", ", x.Details.Teams.Select(t => t.Name)))]);

        var table = tableBuilder.Build();
        TablePrinter.Print(writer, table);
    }

    public void Print(TextWriter writer, LocalReplay resource, int outputMaxWidth)
    {
        if (!resource.Details.Processed)
        {
            writer.WriteLine("Replay has not been processed");
            writer.WriteLine(
                $"Run \"worms process replay {resource.Details.Date:yyyy-MM-dd HH.mm.ss}\" to extract the log");
        }
        else
        {
            writer.WriteLine($"Start Time: {resource.Details.Date:yyyy-MM-dd HH.mm.ss}");
            writer.WriteLine();

            writer.WriteLine("Teams:");
            var teamsTable = new TableBuilder(outputMaxWidth);
            teamsTable.AddColumn("NAME", [.. resource.Details.Teams.Select(x => x.Name)]);
            teamsTable.AddColumn("PLAYER", [.. resource.Details.Teams.Select(x => x.Machine)]);
            teamsTable.AddColumn("COLOUR", [.. resource.Details.Teams.Select(x => x.Colour.ToString())]);
            TablePrinter.Print(writer, teamsTable.Build());
            writer.WriteLine();

            writer.WriteLine("Turns:");
            var turnsTable = new TableBuilder(outputMaxWidth);
            turnsTable.AddColumn(
                "NUM",
                [.. resource.Details.Turns.Select((_, i) => (i + 1).ToString(CultureInfo.CurrentCulture))]);
            turnsTable.AddColumn("TEAM", [.. resource.Details.Turns.Select(x => x.Team.Name)]);
            turnsTable.AddColumn(
                "WEAPONS",
                [.. resource.Details.Turns.Select(x => string.Join(", ", x.Weapons.Select(GetWeaponText)))]);
            turnsTable.AddColumn(
                "DAMAGE",
                [.. resource.Details.Turns.Select(x => string.Join(", ", x.Damage.Select(GetDamageText)))]);
            TablePrinter.Print(writer, turnsTable.Build());
            writer.WriteLine();

            writer.WriteLine("Awards:");
            writer.WriteLine($"Winner: {resource.Details.Winner}");
        }
    }

    private static string GetDamageText(Damage damage)
    {
        var killsText = damage.WormsKilled > 0 ? $" ({damage.WormsKilled} kill{(damage.WormsKilled > 1 ? "s" : "")})" : "";
        return $"{damage.Team.Name}: {damage.HealthLost}{killsText}";
    }

    private static string GetWeaponText(Weapon weapon)
    {
        var details = "";
        var (name, fuse, modifier) = weapon;
        if (fuse != null && modifier != null)
        {
            details = $" ({fuse} sec, {modifier})";
        }
        else if (fuse != null)
        {
            details = $" ({fuse} sec)";
        }
        else if (modifier != null)
        {
            details = $" ({modifier})";
        }

        return name + details;
    }
}

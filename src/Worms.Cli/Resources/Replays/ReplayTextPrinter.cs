using System.Collections.Generic;
using System.IO;
using System.Linq;
using Worms.Armageddon.Files.Replays;
using Worms.Cli.Logging.TableOutput;
using Worms.Cli.Resources.Local.Replays;

namespace Worms.Cli.Resources.Replays
{
    public class ReplayTextPrinter : IResourcePrinter<LocalReplay>
    {
        public void Print(TextWriter writer, IReadOnlyCollection<LocalReplay> items, int outputMaxWidth)
        {
            var tableBuilder = new TableBuilder(outputMaxWidth);

            tableBuilder.AddColumn("NAME", items.Select(x => x.Details.Date.ToString("yyyy-MM-dd HH.mm.ss")).ToList());
            tableBuilder.AddColumn("CONTEXT", items.Select(x => x.Context).ToList());
            tableBuilder.AddColumn("PROCESSED", items.Select(x => x.Details.Processed.ToString()).ToList());
            tableBuilder.AddColumn("WINNER", items.Select(x => x.Details.Winner != null ? x.Details.Winner.ToString() : "").ToList());
            tableBuilder.AddColumn("TEAMS", items.Select(x => string.Join(", ", x.Details.Teams.Select(t => t.Name))).ToList());

            var table = tableBuilder.Build();
            TablePrinter.Print(writer, table);
        }

        public void Print(TextWriter writer, LocalReplay resource, int outputMaxWidth)
        {
            if (!resource.Details.Processed)
            {
                writer.WriteLine("Replay has not been processed");
                writer.WriteLine($"Run \"worms process replay {resource.Details.Date:yyyy-MM-dd HH.mm.ss}\" to extract the log");
            }
            else
            {
                writer.WriteLine($"Start Time: {resource.Details.Date:yyyy-MM-dd HH.mm.ss}");
                writer.WriteLine();

                writer.WriteLine("Teams:");
                var teamsTable = new TableBuilder(outputMaxWidth);
                teamsTable.AddColumn("NAME", resource.Details.Teams.Select(x => x.Name).ToList());
                teamsTable.AddColumn("PLAYER", resource.Details.Teams.Select(x => x.Machine).ToList());
                teamsTable.AddColumn("COLOUR", resource.Details.Teams.Select(x => x.Colour.ToString()).ToList());
                TablePrinter.Print(writer, teamsTable.Build());
                writer.WriteLine();

                writer.WriteLine("Turns:");
                var turnsTable = new TableBuilder(outputMaxWidth);
                turnsTable.AddColumn("NUM", resource.Details.Turns.Select((_, i) => (i+1).ToString()).ToList());
                turnsTable.AddColumn("TEAM", resource.Details.Turns.Select(x => x.Team.Name).ToList());
                turnsTable.AddColumn("WEAPONS", resource.Details.Turns.Select(x => string.Join(", ", x.Weapons.Select(GetWeaponText))).ToList());
                turnsTable.AddColumn("DAMAGE", resource.Details.Turns.Select(x => string.Join(", ", x.Damage.Select(GetDamageText))).ToList());
                TablePrinter.Print(writer, turnsTable.Build());
                writer.WriteLine();

                writer.WriteLine("Awards:");
                writer.WriteLine($"Winner: {resource.Details.Winner}");
            }
        }

        private static string GetDamageText(Damage damage)
        {
            var killsText = damage.WormsKilled > 0 ? $" ({damage.WormsKilled} kill)" : "";
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

            return $"{name}{details}";
        }
    }
}

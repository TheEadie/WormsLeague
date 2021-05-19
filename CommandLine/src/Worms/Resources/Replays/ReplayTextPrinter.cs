using System.Collections.Generic;
using System.IO;
using System.Linq;
using Worms.Armageddon.Resources.Replays;
using Worms.Logging.TableOutput;

namespace Worms.Resources.Replays
{
    public class ReplayTextPrinter : IResourcePrinter<ReplayResource>
    {
        public void Print(TextWriter writer, IReadOnlyCollection<ReplayResource> items, int outputMaxWidth)
        {
            var tableBuilder = new TableBuilder(outputMaxWidth);

            tableBuilder.AddColumn("NAME", items.Select(x => x.Date.ToString("yyyy-MM-dd HH.mm.ss")).ToList());
            tableBuilder.AddColumn("CONTEXT", items.Select(x => x.Context).ToList());
            tableBuilder.AddColumn("PROCESSED", items.Select(x => x.Processed.ToString()).ToList());
            tableBuilder.AddColumn("WINNER", items.Select(x => x.Winner.ToString()).ToList());
            tableBuilder.AddColumn("TEAMS", items.Select(x => string.Join(", ", x.Teams.Select(t => t.Name))).ToList());

            var table = tableBuilder.Build();
            TablePrinter.Print(writer, table);
        }

        public void Print(TextWriter writer, ReplayResource resource, int outputMaxWidth)
        {
            if (!resource.Processed)
            {
                writer.WriteLine("Replay has not been processed");
                writer.WriteLine($"Run \"worms process replay {resource.Date:yyyy-MM-dd HH.mm.ss}\" to extract the log");
            }
            else
            {
                writer.WriteLine($"Start Time: {resource.Date:yyyy-MM-dd HH.mm.ss}");
                writer.WriteLine();

                writer.WriteLine("Teams:");
                var teamsTable = new TableBuilder(outputMaxWidth);
                teamsTable.AddColumn("NAME", resource.Teams.Select(x => x.Name).ToList());
                teamsTable.AddColumn("PLAYER", resource.Teams.Select(x => x.Machine).ToList());
                teamsTable.AddColumn("COLOUR", resource.Teams.Select(x => x.Colour.ToString()).ToList());
                TablePrinter.Print(writer, teamsTable.Build());
                writer.WriteLine();

                writer.WriteLine("Turns:");
                var turnsTable = new TableBuilder(outputMaxWidth);
                turnsTable.AddColumn("NUM", resource.Turns.Select((_, i) => (i+1).ToString()).ToList());
                turnsTable.AddColumn("TEAM", resource.Turns.Select(x => x.Team.Name).ToList());
                turnsTable.AddColumn("WEAPONS", resource.Turns.Select(x => string.Join(", ", x.Weapons.Select(t => t.Name))).ToList());
                turnsTable.AddColumn("DAMAGE", resource.Turns.Select(x => string.Join(", ", x.Damage.Select(GetDamageText))).ToList());
                TablePrinter.Print(writer, turnsTable.Build());
                writer.WriteLine();
            }
        }

        private string GetDamageText(Damage damage)
        {
            var killsText = damage.WormsKilled > 0 ? $" ({damage.WormsKilled} kill)" : "";
            return $"{damage.Team.Name}: {damage.HealthLost}{killsText}";
        }
    }
}

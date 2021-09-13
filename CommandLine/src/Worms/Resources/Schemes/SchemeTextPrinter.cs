using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Resources.Schemes.Text;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Logging.TableOutput;

namespace Worms.Resources.Schemes
{
    internal class SchemeTextPrinter : IResourcePrinter<LocalScheme>
    {
        private readonly ISchemeTextWriter _schemeTextWriter;

        public SchemeTextPrinter(ISchemeTextWriter schemeTextWriter)
        {
            _schemeTextWriter = schemeTextWriter;
        }

        public void Print(TextWriter writer, IReadOnlyCollection<LocalScheme> items, int outputMaxWidth)
        {
            var tableBuilder = new TableBuilder(outputMaxWidth);
            tableBuilder.AddColumn("NAME", items.Select(x => x.Name).ToList());
            tableBuilder.AddColumn("CONTEXT", items.Select(x => x.Context).ToList());
            tableBuilder.AddColumn("HEALTH", items.Select(x => x.Details.WormEnergy.ToString()).ToList());
            tableBuilder.AddColumn(
                "TURN-TIME",
                items.Select(x => GetTurnTime(x.Details.TurnTimeInfinite, x.Details.TurnTime)).ToList());
            tableBuilder.AddColumn(
                "ROUND-TIME",
                items.Select(x => GetRoundTime(x.Details.RoundTimeMinutes, x.Details.RoundTimeSeconds)).ToList());
            tableBuilder.AddColumn("WORM-SELECT", items.Select(x => x.Details.WormSelect.ToString()).ToList());
            tableBuilder.AddColumn("WEAPONS", items.Select(x => GetWeaponSummary(x.Details.Weapons)).ToList());

            var table = tableBuilder.Build();
            TablePrinter.Print(writer, table);
        }

        public void Print(TextWriter writer, LocalScheme resource, int outputMaxWidth)
        {
            _schemeTextWriter.Write(resource.Details, writer);
        }

        private static string GetTurnTime(bool infinite, byte seconds)
        {
            return infinite ? "Infinite" : seconds + " secs";
        }

        private static string GetRoundTime(byte minutes, byte seconds)
        {
            return seconds > 0 ? seconds + " secs" : minutes + " mins";
        }

        private static string GetWeaponSummary(Scheme.WeaponList weapons)
        {
            var summary = "";
            foreach (var weaponName in (Weapon[])Enum.GetValues(typeof(Weapon)))
            {
                var weapon = weapons[weaponName];
                if (weapon.Ammo > 0)
                {
                    summary += $"{weaponName.ToString().Substring(0,2)}({weapon.Ammo}), ";
                }
            }

            if (summary.Length > 0)
            {
                summary = summary.Substring(0, summary.Length - 2);
            }

            return summary;
        }


    }
}

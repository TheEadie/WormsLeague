using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syroot.Worms.Armageddon;
using Worms.Logging.TableOutput;
using Worms.Resources.Schemes;

namespace Worms.Logging
{
    internal class DefaultSchemesPrinter : IResourcePrinter<SchemeResource>
    {
        public void Print(TextWriter writer, IReadOnlyCollection<SchemeResource> items, int outputMaxWidth)
        {
            var tableBuilder = new TableBuilder(outputMaxWidth);
            tableBuilder.AddColumn("NAME", items.Select(x => x.Name).ToList());
            tableBuilder.AddColumn("CONTEXT", items.Select(x => x.Context).ToList());
            tableBuilder.AddColumn("HEALTH", items.Select(x => x.Details.WormEnergy.ToString()).ToList());
            tableBuilder.AddColumn("TURN-TIME", items.Select(x => x.Details.TurnTime + " secs").ToList());
            tableBuilder.AddColumn("ROUND-TIME", items.Select(x => x.Details.RoundTimeMinutes + " mins").ToList());
            tableBuilder.AddColumn("WORM-SELECT", items.Select(x => x.Details.WormSelect.ToString()).ToList());
            tableBuilder.AddColumn("WEAPONS", items.Select(x => GetWeaponSummary(x.Details.Weapons)).ToList());

            var table = tableBuilder.Build();
            TablePrinter.Print(writer, table);
        }

        private static string GetWeaponSummary(Scheme.WeaponList weapons)
        {
            var summary = "";
            foreach (var weaponName in (Weapon[])Enum.GetValues(typeof(Weapon)))
            {
                var weapon = weapons[weaponName];
                if (weapon.Ammo > 0)
                {
                    summary += $"{weaponName.ToString().Substring(0,2)} ({weapon.Ammo}) ,";
                }
            }

            return summary;
        }

        public void Print(TextWriter writer, SchemeResource item, int outputMaxWidth)
        {
            WriteHeader(writer, "GENERAL");
            WriteItem(writer, "Hot seat delay", item.Details.HotSeatTime);
            WriteItem(writer, "Retreat time", item.Details.RetreatTime, "Seconds");
            WriteItem(writer, "Rope retreat time", item.Details.RetreatTimeRope, "Seconds");
            WriteItem(writer, "Display total round time", item.Details.ShowRoundTime);
            WriteItem(writer, "Automatic replays", item.Details.Replays);
            WriteItem(writer, "Fall damage", item.Details.FallDamage);
            WriteItem(writer, "Artillery mode", item.Details.ArtilleryMode, "Worms can't move");
            WriteItem(
                writer,
                "Stockpiling mode",
                item.Details.Stockpiling,
                "0 = Replenishing, 1 = Accumulating, 2 = Reducing");
            WriteItem(writer, "Worms select", item.Details.WormSelect, "0 = Off, 1 = On, 2 = Random");
            WriteItem(
                writer,
                "Sudden death event",
                item.Details.SuddenDeathEvent,
                "0 = Leader wins, 1 = Nuclear strike, 2 = HP reduced to 1, 3 = Nothing");
            WriteItem(
                writer,
                "Sudden death water rise rate",
                item.Details.WaterRiseRate,
                "See table on http://worms2d.info/Sudden_Death");
            WriteItem(
                writer,
                "Weapon crate probability",
                item.Details.WeaponCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(
                writer,
                "Health crate probability",
                item.Details.HealthCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(
                writer,
                "Utility crate probability",
                item.Details.UtilityCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(writer, "Health crate energy", item.Details.HealthCrateEnergy);
            WriteItem(writer, "Donor cards", item.Details.DonorCards);
            WriteItem(
                writer,
                "Hazard objects",
                item.Details.ObjectTypes,
                "Stores type and number See http://worms2d.info/Hazardous_Objects");
            WriteItem(writer, "Mine delay", item.Details.MineDelay);
            WriteItem(writer, "Dud mines", item.Details.DudMines);
            WriteItem(writer, "Worm placement", item.Details.DudMines, "0 = Auto, 1 = Manual");
            WriteItem(writer, "Initial worm energy", item.Details.WormEnergy);
            WriteItem(writer, "Turn time", item.Details.TurnTime, "Seconds, >180 = unlimited");
            WriteItem(writer, "Round time", item.Details.RoundTimeMinutes, "Minutes, >180 = x-180 Seconds");
            WriteItem(writer, "Number of rounds", item.Details.NumberOfWins);
            WriteItem(writer, "Blood", item.Details.Blood);
            WriteItem(writer, "Aqua sheep", item.Details.AquaSheep);
            WriteItem(
                writer,
                "Sheep heaven",
                item.Details.SheepHeaven,
                "Exploding sheep jump out of destroyed weapon crates");
            WriteItem(writer, "God worms", item.Details.GodWorms, "Worms can't lose health");
            WriteItem(writer, "Indestructible land", item.Details.IndiLand);
            WriteItem(writer, "Upgraded grenade", item.Details.UpgradeGrenade);
            WriteItem(writer, "Upgraded shotgun", item.Details.UpgradeShotgun);
            WriteItem(writer, "Upgraded cluster bombs", item.Details.UpgradeCluster);
            WriteItem(writer, "Upgraded longbow", item.Details.UpgradeLongbow);
            WriteItem(
                writer,
                "Team weapons",
                item.Details.TeamWeapons,
                "Teams will start the match with their preselected team weapon");
            WriteItem(writer, "Super weapons", item.Details.SuperWeapons, "Super weapons may appear in crates");
            writer.WriteLine();

            WriteHeader(writer, "WEAPONS");
            writer.WriteLine("(See http://worms2d.info/Weapons for what various power settings will do)");
            writer.WriteLine();

            foreach (var weaponName in (Weapon[])Enum.GetValues(typeof(Weapon)))
            {
                var weapon = item.Details.Weapons[weaponName];
                var ammoPadding = weapon.Ammo > 9 ? "   " : "    ";
                var powerPadding = weapon.Power > 9 ? "   " : "    ";
                var delayPadding = weapon.Delay > 9 ? "   " : "    ";
                writer.WriteLine(
                    weaponName.ToString().PadRight(30)
                    + "Ammo: ["
                    + weapon.Ammo
                    + "]"
                    + ammoPadding
                    + "Power: ["
                    + weapon.Power
                    + "]"
                    + powerPadding
                    + "Delay: ["
                    + weapon.Delay
                    + "]"
                    + delayPadding
                    + "Crate probability: ["
                    + weapon.Prob
                    + "]");
            }
        }

        private static void WriteItem(TextWriter writer, string description, object value, string comment = null)
        {
            var output = $"{description}: ".PadRight(40) + "[" + value + "]";

            if (comment != null)
            {
                output += $" ({comment})";
            }

            writer.WriteLine(output);
        }

        private static void WriteHeader(TextWriter writer, string heading)
        {
            writer.WriteLine("///////////////////");
            writer.WriteLine($"// {heading}".PadRight(17) + "//");
            writer.WriteLine("///////////////////");
            writer.WriteLine("");
        }
    }
}

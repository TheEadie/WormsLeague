using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            tableBuilder.AddColumn("HEALTH", items.Select(x => x.Details.InitialWormEnergy.ToString()).ToList());
            tableBuilder.AddColumn("TURN-TIME", items.Select(x => x.Details.TurnTime + " secs").ToList());
            tableBuilder.AddColumn("ROUND-TIME", items.Select(x => x.Details.RoundTime + " mins").ToList());
            tableBuilder.AddColumn("WORM-SELECT", items.Select(x => (x.Details.WormSelect == 1).ToString()).ToList());
            tableBuilder.AddColumn(
                "WEAPONS",
                items.Select(
                        x => string.Join(
                            ", ",
                            x.Details.Weapons.Where(w => w.Ammo > 0)
                                .Select(w => w.Name.Substring(0, 2) + " (" + w.Ammo + ")")))
                    .ToList());

            var table = tableBuilder.Build();
            TablePrinter.Print(writer, table);
        }

        public void Print(TextWriter writer, SchemeResource item, int outputMaxWidth)
        {
            WriteHeader(writer, "GENERAL");
            WriteItem(writer, "Hot seat delay", item.Details.HotSeatDelay);
            WriteItem(writer, "Retreat time", item.Details.RetreatTime, "Seconds");
            WriteItem(writer, "Rope retreat time", item.Details.RopeRetreatTime, "Seconds");
            WriteItem(writer, "Display total round time", item.Details.DisplayTotalRoundTime);
            WriteItem(writer, "Automatic replays", item.Details.AutomaticReplays);
            WriteItem(writer, "Fall damage", item.Details.FallDamage);
            WriteItem(writer, "Artillery mode", item.Details.ArtilleryMode, "Worms can't move");
            WriteItem(
                writer,
                "Stockpiling mode",
                item.Details.StockpilingMode,
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
                item.Details.WeaponCrateProbability,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(
                writer,
                "Health crate probability",
                item.Details.HealthCrateProbability,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(
                writer,
                "Utility crate probability",
                item.Details.UtilityCrateProbability,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(writer, "Health crate energy", item.Details.HealthCrateEnergy);
            WriteItem(writer, "Donor cards", item.Details.DonorCards);
            WriteItem(
                writer,
                "Hazard objects",
                item.Details.HazardObjectTypes,
                "Stores type and number See http://worms2d.info/Hazardous_Objects");
            WriteItem(writer, "Mine delay", item.Details.MineDelay);
            WriteItem(writer, "Dud mines", item.Details.DudMines);
            WriteItem(writer, "Worm placement", item.Details.DudMines, "0 = Auto, 1 = Manual");
            WriteItem(writer, "Initial worm energy", item.Details.InitialWormEnergy);
            WriteItem(writer, "Turn time", item.Details.TurnTime, "Seconds, >180 = unlimited");
            WriteItem(writer, "Round time", item.Details.RoundTime, "Minutes, >180 = x-180 Seconds");
            WriteItem(writer, "Number of rounds", item.Details.NumberOfRounds);
            WriteItem(writer, "Blood", item.Details.Blood);
            WriteItem(writer, "Aqua sheep", item.Details.AquaSheep);
            WriteItem(
                writer,
                "Sheep heaven",
                item.Details.SheepHeaven,
                "Exploding sheep jump out of destroyed weapon crates");
            WriteItem(writer, "God worms", item.Details.GodWorms, "Worms can't lose health");
            WriteItem(writer, "Indestructible land", item.Details.IndestructibleLand);
            WriteItem(writer, "Upgraded grenade", item.Details.UpgradedGrenade);
            WriteItem(writer, "Upgraded shotgun", item.Details.UpgradedShotgun);
            WriteItem(writer, "Upgraded cluster bombs", item.Details.UpgradedClusterBombs);
            WriteItem(writer, "Upgraded longbow", item.Details.UpgradedLongbow);
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

            foreach (var weapon in item.Details.Weapons)
            {
                var ammoPadding = weapon.Ammo > 9 ? "   " : "    ";
                var powerPadding = weapon.Power > 9 ? "   " : "    ";
                var delayPadding = weapon.Delay > 9 ? "   " : "    ";
                writer.WriteLine(
                    weapon.Name.PadRight(30)
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
                    + weapon.CrateProbability
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

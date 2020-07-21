using System;
using System.IO;
using Syroot.Worms.Armageddon;

namespace Worms.Resources.Schemes.Text
{
    internal class SchemeTextWriter : ISchemeTextWriter
    {
        public void Write(Scheme definition, TextWriter textWriter)
        {
            WriteHeader(textWriter, "GENERAL");
            WriteItem(textWriter, "Hot seat delay", definition.HotSeatTime, "Seconds");
            WriteItem(textWriter, "Retreat time", definition.RetreatTime, "Seconds");
            WriteItem(textWriter, "Rope retreat time", definition.RetreatTimeRope, "Seconds");
            WriteItem(textWriter, "Display total round time", definition.ShowRoundTime);
            WriteItem(textWriter, "Automatic replays", definition.Replays);
            WriteItem(textWriter, "Fall damage", definition.FallDamage);
            WriteItem(textWriter, "Artillery mode", definition.ArtilleryMode, "Worms can't move");
            WriteItem(
                textWriter,
                "Stockpiling mode",
                definition.Stockpiling,
                "Off | On | Anti");
            WriteItem(textWriter, "Worms select", definition.WormSelect, "Sequential | Manual | Random");
            WriteItem(
                textWriter,
                "Sudden death event",
                definition.SuddenDeathEvent,
                "RoundEnd | NuclearStrike | HealthDrop | WaterRise");
            WriteItem(
                textWriter,
                "Sudden death water rise rate",
                definition.WaterRiseRate,
                "See table on http://worms2d.info/Sudden_Death");
            WriteItem(
                textWriter,
                "Weapon crate probability",
                definition.WeaponCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(
                textWriter,
                "Health crate probability",
                definition.HealthCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(
                textWriter,
                "Utility crate probability",
                definition.UtilityCrateProb,
                "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(textWriter, "Health crate energy", definition.HealthCrateEnergy);
            WriteItem(textWriter, "Donor cards", definition.DonorCards);
            WriteItem(textWriter, "Hazard objects", definition.ObjectTypes, "None | Mines | OilDrums | Both");
            WriteItem(textWriter, "Max num of hazard objects", definition.ObjectCount);
            WriteItem(textWriter, "Random mine delay", definition.MineDelayRandom, "If set mine delay will be ignored");
            WriteItem(textWriter, "Mine delay", definition.MineDelay);
            WriteItem(textWriter, "Dud mines", definition.DudMines);
            WriteItem(textWriter, "Worm placement", definition.ManualWormPlacement);
            WriteItem(textWriter, "Initial worm energy", definition.WormEnergy);
            WriteItem(textWriter, "Infinite turn time", definition.TurnTimeInfinite, "If set turn time will be ignored");
            WriteItem(textWriter, "Turn time", definition.TurnTime, "Seconds");
            WriteItem(textWriter, "Round time (mins)", definition.RoundTimeMinutes, "Minutes");
            WriteItem(textWriter, "Round time (secs)", definition.RoundTimeSeconds, "Seconds");
            WriteItem(textWriter, "Number of rounds", definition.NumberOfWins);
            WriteItem(textWriter, "Blood", definition.Blood);
            WriteItem(textWriter, "Aqua sheep", definition.AquaSheep);
            WriteItem(
                textWriter,
                "Sheep heaven",
                definition.SheepHeaven,
                "Exploding sheep jump out of destroyed weapon crates");
            WriteItem(textWriter, "God worms", definition.GodWorms, "Worms can't lose health");
            WriteItem(textWriter, "Indestructible land", definition.IndiLand);
            WriteItem(textWriter, "Upgraded grenade", definition.UpgradeGrenade);
            WriteItem(textWriter, "Upgraded shotgun", definition.UpgradeShotgun);
            WriteItem(textWriter, "Upgraded cluster bombs", definition.UpgradeCluster);
            WriteItem(textWriter, "Upgraded longbow", definition.UpgradeLongbow);
            WriteItem(
                textWriter,
                "Team weapons",
                definition.TeamWeapons,
                "Teams will start the match with their preselected team weapon");
            WriteItem(textWriter, "Super weapons", definition.SuperWeapons, "Super weapons may appear in crates");
            textWriter.WriteLine();

            WriteHeader(textWriter, "WEAPONS");
            textWriter.WriteLine("(See http://worms2d.info/Weapons for what various power settings will do)");
            textWriter.WriteLine();

            foreach (var weaponName in (Weapon[])Enum.GetValues(typeof(Weapon)))
            {
                var weapon = definition.Weapons[weaponName];
                var ammoPadding = weapon.Ammo > 9 ? "   " : "    ";
                var powerPadding = weapon.Power > 9 ? "   " : "    ";
                var delayPadding = weapon.Delay > 9 ? "   " : "    ";
                textWriter.WriteLine(
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

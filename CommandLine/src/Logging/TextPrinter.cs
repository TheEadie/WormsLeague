using Serilog;
using Worms.Resources.Schemes;

namespace Worms.Logging
{
    internal class TextPrinter
    {
        public void Print(ILogger logger, SchemeResource item)
        {
            WriteHeader(logger, "GENERAL");
            WriteItem(logger, "Name", item.Name);
            WriteItem(logger, "Context", item.Context);
            logger.Information("");
            WriteItem(logger, "Hot seat delay", item.Details.HotSeatDelay);
            WriteItem(logger, "Retreat time", item.Details.RetreatTime, "Seconds");
            WriteItem(logger, "Rope retreat time", item.Details.RopeRetreatTime, "Seconds");
            WriteItem(logger, "Display total round time", item.Details.DisplayTotalRoundTime);
            WriteItem(logger, "Automatic replays", item.Details.AutomaticReplays);
            WriteItem(logger, "Fall damage", item.Details.FallDamage);
            WriteItem(logger, "Artillery mode", item.Details.ArtilleryMode, "Worms can't move");
            WriteItem(logger, "Stockpiling mode", item.Details.StockpilingMode, "0 = Replenishing, 1 = Accumulating, 2 = Reducing");
            WriteItem(logger, "Worms select", item.Details.WormSelect, "0 = Off, 1 = On, 2 = Random");
            WriteItem(logger, "Sudden death event", item.Details.SuddenDeathEvent, "0 = Leader wins, 1 = Nuclear strike, 2 = HP reduced to 1, 3 = Nothing");
            WriteItem(logger, "Sudden death water rise rate", item.Details.WaterRiseRate, "See table on http://worms2d.info/Sudden_Death");
            WriteItem(logger, "Weapon crate probability", item.Details.WeaponCrateProbability, "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(logger, "Health crate probability", item.Details.HealthCrateProbability, "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(logger, "Utility crate probability", item.Details.UtilityCrateProbability, "0-100, See http://worms2d.info/Crate_probability");
            WriteItem(logger, "Health crate energy", item.Details.HealthCrateEnergy);
            WriteItem(logger, "Donor cards", item.Details.DonorCards);
            WriteItem(logger, "Hazard objects", item.Details.HazardObjectTypes, "Stores type and number See http://worms2d.info/Hazardous_Objects");
            WriteItem(logger, "Mine delay", item.Details.MineDelay);
            WriteItem(logger, "Dud mines", item.Details.DudMines);
            WriteItem(logger, "Worm placement", item.Details.DudMines, "0 = Auto, 1 = Manual");
            WriteItem(logger, "Initial worm energy", item.Details.InitialWormEnergy);
            WriteItem(logger, "Turn time", item.Details.TurnTime, "Seconds, >180 = unlimited");
            WriteItem(logger, "Round time", item.Details.RoundTime, "Minutes, >180 = x-180 Seconds");
            WriteItem(logger, "Number of rounds", item.Details.NumberOfRounds);
            WriteItem(logger, "Blood", item.Details.Blood);
            WriteItem(logger, "Aqua sheep", item.Details.AquaSheep);
            WriteItem(logger, "Sheep heaven", item.Details.SheepHeaven, "Exploding sheep jump out of destroyed weapon crates");
            WriteItem(logger, "God worms", item.Details.GodWorms, "Worms can't lose health");
            WriteItem(logger, "Indestructible land", item.Details.IndestructibleLand);
            WriteItem(logger, "Upgraded grenade", item.Details.UpgradedGrenade);
            WriteItem(logger, "Upgraded shotgun", item.Details.UpgradedShotgun);
            WriteItem(logger, "Upgraded cluster bombs", item.Details.UpgradedClusterBombs);
            WriteItem(logger, "Upgraded longbow", item.Details.UpgradedLongbow);
            WriteItem(logger, "Team weapons", item.Details.TeamWeapons, "Teams will start the match with their preselected team weapon");
            WriteItem(logger, "Super weapons", item.Details.SuperWeapons, "Super weapons may appear in crates");
            logger.Information("");

            WriteHeader(logger, "WEAPONS");
            logger.Information("(See http://worms2d.info/Weapons for what various power settings will do)");
            logger.Information("");

            foreach (var weapon in item.Details.Weapons)
            {
                var ammoPadding = weapon.Ammo > 9 ? "   " : "    ";
                var powerPadding = weapon.Power > 9 ? "   " : "    ";
                var delayPadding = weapon.Delay > 9 ? "   " : "    ";
                logger.Information(weapon.Name.PadRight(30) +
                    "Ammo: [" + weapon.Ammo + "]" + ammoPadding +
                    "Power: [" + weapon.Power + "]" + powerPadding +
                    "Delay: [" + weapon.Delay + "]" + delayPadding +
                    "Crate probability: [" + weapon.CrateProbability + "]");
            }
        }

        private void WriteItem(ILogger logger, string description, object value, string comment = null)
        {
            var output = $"{description}: ".PadRight(40) + "[" + SeriiLogEscape(value.ToString()) + "]";

            if (comment != null)
            {
                output += $" ({comment})";
            }

            logger.Information(output);
        }

        private void WriteHeader(ILogger logger, string heading)
        {
            logger.Information("///////////////////");
            logger.Information($"// {heading}".PadRight(17) + "//");
            logger.Information("///////////////////");
            logger.Information("");
        }

        private string SeriiLogEscape(string input)
        {
            // Special case for text like {{01}} which appears in the default schipped worms scheme names
            // This needs a more general fix to tell seriilog not to treat anything as a special char
            return input.Replace("{{", "{{{").Replace("}}", "}}}");
        }
    }
}
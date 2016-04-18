using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WormsScheme
{
    public class TextFileWriter
    {
        private readonly string m_FilePath;

        public TextFileWriter(string filePath)
        {
            m_FilePath = filePath;
        }

        public void WriteModel(Scheme model)
        {
            using (var b = new StreamWriter(File.Open(m_FilePath, FileMode.Create)))
            {
                b.WriteLine("WSC version [" + model.Version + "]");
                b.WriteLine("///////////////////");
                b.WriteLine("// GENERAL       //");
                b.WriteLine("///////////////////");
                b.WriteLine("");
                b.WriteLine("Hot seat delay: ".PadRight(40) + "[" + model.HotSeatDelay + "]" + " (Seconds)");
                b.WriteLine("Retreat time: ".PadRight(40) + "[" + model.RetreatTime + "]" + " (Seconds)");
                b.WriteLine("Rope retreat time: ".PadRight(40) + "[" + model.RopeRetreatTime + "]" + " (Seconds)");
                b.WriteLine("Display total round time: ".PadRight(40) + "[" + model.DisplayTotalRoundTime + "]" + "");
                b.WriteLine("Automatic replays: ".PadRight(40) + "[" + model.AutomaticReplays + "]" + "");
                b.WriteLine("Fall damage: ".PadRight(40) + "[" + model.FallDamage + "]" + "");
                b.WriteLine("Artillery mode: ".PadRight(40) + "[" + model.ArtilleryMode + "]" + " (Worms can't move)");
                b.WriteLine("Stockpiling mode: ".PadRight(40) + "[" + model.StockpilingMode + "]" + " (0 = Replenishing, 1 = Accumulating, 2 = Reducing)");
                b.WriteLine("Worms select: ".PadRight(40) + "[" + model.WormSelect + "]" + " (0 = Off, 1 = On, 2 = Random)");
                b.WriteLine("Sudden death event: ".PadRight(40) + "[" + model.SuddenDeathEvent + "]" + " (0 = Leader wins, 1 = Nuclear strike, 2 = HP reduced to 1, 3 = Nothing)");
                b.WriteLine("Sudden death water rise rate: ".PadRight(40) + "[" + model.WaterRiseRate + "]" + " (See table on http://worms2d.info/Sudden_Death)");
                b.WriteLine("Weapon crate probability: ".PadRight(40) + "[" + model.WeaponCrateProbability + "]" + " (0-100, See http://worms2d.info/Crate_probability)");
                b.WriteLine("Health crate probability: ".PadRight(40) + "[" + model.HealthCrateProbability + "]" + " (0-100, See http://worms2d.info/Crate_probability)");
                b.WriteLine("Utility crate probability: ".PadRight(40) + "[" + model.UtilityCrateProbability + "]" + " (0-100, See http://worms2d.info/Crate_probability)");
                b.WriteLine("Health crate energy: ".PadRight(40) + "[" + model.HealthCrateEnergy + "]" + "");
                b.WriteLine("Donor cards: ".PadRight(40) + "[" + model.DonorCards + "]" + "");
                b.WriteLine("Hazard objects: ".PadRight(40) + "[" + model.HazardObjectTypes + "]" + " (Stores type and number See http://worms2d.info/Hazardous_Objects)");
                b.WriteLine("Mine delay: ".PadRight(40) + "[" + model.MineDelay + "]" + "");
                b.WriteLine("Dud mines: ".PadRight(40) + "[" + model.DudMines + "]" + "");
                b.WriteLine("Worm placement: ".PadRight(40) + "[" + model.DudMines + "]" + " (0 = Auto, 1 = Manual)");
                b.WriteLine("Initial worm energy: ".PadRight(40) + "[" + model.InitialWormEnergy + "]" + "");
                b.WriteLine("Turn time: ".PadRight(40) + "[" + model.TurnTime + "]" + " (Seconds, >180 = unlimited)");
                b.WriteLine("Round time: ".PadRight(40) + "[" + model.RoundTime + "]" + " (Minutes, >180 = x-180 Seconds)");
                b.WriteLine("Number of rounds: ".PadRight(40) + "[" + model.NumberOfRounds + "]" + "");
                b.WriteLine("Blood: ".PadRight(40) + "[" + model.Blood + "]" + "");
                b.WriteLine("Aqua sheep: ".PadRight(40) + "[" + model.AquaSheep + "]" + "");
                b.WriteLine("Sheep heaven: ".PadRight(40) + "[" + model.SheepHeaven + "]" + " (Exploding sheep jump out of destroyed weapon crates)");
                b.WriteLine("God worms: ".PadRight(40) + "[" + model.GodWorms + "]" + " (Worms can't lose health)");
                b.WriteLine("Indestructible land: ".PadRight(40) + "[" + model.IndestructibleLand + "]" + "");
                b.WriteLine("Upgraded grenade: ".PadRight(40) + "[" + model.UpgradedGrenade + "]" + "");
                b.WriteLine("Upgraded shotgun: ".PadRight(40) + "[" + model.UpgradedShotgun + "]" + "");
                b.WriteLine("Upgraded cluster bombs: ".PadRight(40) + "[" + model.UpgradedClusterBombs + "]" + "");
                b.WriteLine("Upgraded longbow: ".PadRight(40) + "[" + model.UpgradedLongbow + "]" + "");
                b.WriteLine("Team weapons: ".PadRight(40) + "[" + model.TeamWeapons + "]" + " (Teams will start the match with their preselected team weapon)");
                b.WriteLine("Super weapons: ".PadRight(40) + "[" + model.SuperWeapons + "]" + " (Super weapons may appear in crates)");
                b.WriteLine("");

                b.WriteLine("///////////////////");
                b.WriteLine("// WEAPONS       //");
                b.WriteLine("///////////////////");
                b.WriteLine("");
                b.WriteLine("(See http://worms2d.info/Weapons for what various power settings will do)");
                b.WriteLine("");

                foreach (var weapon in model.Weapons)
                {
                    var ammoPadding = weapon.Ammo > 9 ? "   " : "    ";
                    var powerPadding = weapon.Power > 9 ? "   " : "    ";
                    var delayPadding = weapon.Delay > 9 ? "   " : "    ";
                    b.WriteLine(weapon.Name.PadRight(30) + 
                        "Ammo: " + "[" + weapon.Ammo + "]" + ammoPadding + 
                        "Power: " + "[" + weapon.Power + "]" + powerPadding + 
                        "Delay: " + "[" + weapon.Delay + "]" + delayPadding +
                        "Crate probability: " + "[" + weapon.CrateProbability + "]");
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WormsScheme.Model;

namespace WormsScheme
{
    public class WscWriter
    {
        private readonly string m_FilePath;

        public WscWriter(string filePath)
        {
            m_FilePath = filePath;
        }

        public void WriteModel(Scheme model)
        {
            using (var b = new BinaryWriter(File.Open(m_FilePath, FileMode.Create)))
            {
                b.Write(model.Signature.ToCharArray(0,4));
                b.Write(model.Version);
                b.Write(model.HotSeatDelay);
                b.Write(model.RetreatTime);
                b.Write(model.RopeRetreatTime);
                b.Write(model.DisplayTotalRoundTime);
                b.Write(model.AutomaticReplays);
                b.Write(model.FallDamage);
                b.Write(model.ArilleryMode);
                b.Write(0);
                b.Write(model.StockpilingMode);
                b.Write(model.WormSelect);
                b.Write(model.SuddenDeathEvent);
                b.Write(model.WaterRiseRate);
                b.Write(model.WeaponCrateProbability);
                b.Write(model.DonorCards);
                b.Write(model.HealthCrateProbability);
                b.Write(model.HealthCrateEnergy);
                b.Write(model.UtilityCrateProbability);
                b.Write(model.HazardObjectTypes);
                b.Write(model.MineDelay);
                b.Write(model.DudMines);
                b.Write(model.WormPlacement);
                b.Write(model.InitialWormEnergy);
                b.Write(model.TurnTime);
                b.Write(model.RoundTime);
                b.Write(model.NumberOfRounds);
                b.Write(model.Blood);
                b.Write(model.AquaSheep);
                b.Write(model.SheepHeaven);
                b.Write(model.GodWorms);
                b.Write(model.IndestructibleLand);
                b.Write(model.UpgradedGrenade);
                b.Write(model.UpgradedShotgun);
                b.Write(model.UpgradedClusterBombs);
                b.Write(model.UpgradedLongbow);
                b.Write(model.TeamWeapons);
                b.Write(model.SuperWeapons);
                
                foreach (var weapon in model.Weapons)
                {
                    b.Write(weapon.Ammo);
                    b.Write(weapon.Power);
                    b.Write(weapon.Delay);
                    b.Write(weapon.CrateProbability);
                }
            }
        }
    }
}

using System.IO;
using System.Linq;

namespace WormsRandomizer.WormsScheme
{
    public class WscWriter : IWscWriter
    {
        public void Write(IReadOnlyScheme scheme, string filePath)
        {
            using (var b = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                b.Write(scheme.Signature.ToCharArray(0,4));
                b.Write((byte)scheme.Version);
                b.Write((byte)scheme.HotSeatDelay);
                b.Write((byte)scheme.RetreatTime);
                b.Write((byte)scheme.RopeRetreatTime);
                b.Write(scheme.DisplayTotalRoundTime);
                b.Write(scheme.AutomaticReplays);
                b.Write((byte)scheme.FallDamage);
                b.Write(scheme.ArtilleryMode);
                b.Write((byte)0);
                b.Write(scheme.StockpilingMode);
                b.Write(scheme.WormSelect);
                b.Write(scheme.SuddenDeathEvent);
                b.Write((byte)scheme.WaterRiseRate);
                b.Write((byte)scheme.WeaponCrateProbability);
                b.Write(scheme.DonorCards);
                b.Write((byte)scheme.HealthCrateProbability);
                b.Write((byte)scheme.HealthCrateEnergy);
                b.Write((byte)scheme.UtilityCrateProbability);
                b.Write((byte)scheme.HazardObjectTypes);
                b.Write((byte)scheme.MineDelay);
                b.Write(scheme.DudMines);
                b.Write(scheme.WormPlacement);
                b.Write((byte)scheme.InitialWormEnergy);
                b.Write((byte)scheme.TurnTime);
                b.Write((byte)scheme.RoundTime);
                b.Write((byte)scheme.NumberOfRounds);
                b.Write(scheme.Blood);
                b.Write(scheme.AquaSheep);
                b.Write(scheme.SheepHeaven);
                b.Write(scheme.GodWorms);
                b.Write(scheme.IndestructibleLand);
                b.Write(scheme.UpgradedGrenade);
                b.Write(scheme.UpgradedShotgun);
                b.Write(scheme.UpgradedClusterBombs);
                b.Write(scheme.UpgradedLongbow);
                b.Write(scheme.TeamWeapons);
                b.Write(scheme.SuperWeapons);
                
                foreach (var weaponName in Weapons.AllWeapons)
                {
                    var weapon = scheme.WeaponInfo.First(x => x.Name == weaponName);
                    b.Write((byte)weapon.Ammo);
                    b.Write((byte)(weapon.Power-1));
                    b.Write((byte)weapon.Delay);
                    b.Write((byte)weapon.CrateProbability);
                }
            }
        }
    }
}

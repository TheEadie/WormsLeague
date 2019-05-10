using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WormsRandomizer.WormsScheme
{
    public class WscReader : IWscReader
    {
        public IReadOnlyScheme Read(string filePath)
        {
            var scheme = new Scheme();
            using (var b = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                scheme.Signature = new string(b.ReadChars(4));
                scheme.Version = b.ReadByte();
                scheme.HotSeatDelay = b.ReadByte();
                scheme.RetreatTime = b.ReadByte();
                scheme.RopeRetreatTime = b.ReadByte();
                scheme.DisplayTotalRoundTime = b.ReadBoolean();
                scheme.AutomaticReplays = b.ReadBoolean();
                scheme.FallDamage = b.ReadByte();
                scheme.ArtilleryMode = b.ReadBoolean();
                b.ReadBoolean(); //UnusedBountyMode
                scheme.StockpilingMode = b.ReadByte();
                scheme.WormSelect = b.ReadByte();
                scheme.SuddenDeathEvent = b.ReadByte();
                scheme.WaterRiseRate = b.ReadSByte();
                scheme.WeaponCrateProbability = b.ReadSByte();
                scheme.DonorCards = b.ReadBoolean();
                scheme.HealthCrateProbability = b.ReadSByte();
                scheme.HealthCrateEnergy = b.ReadByte();
                scheme.UtilityCrateProbability = b.ReadSByte();
                scheme.HazardObjectTypes = b.ReadByte();
                scheme.MineDelay = b.ReadByte();
                scheme.DudMines = b.ReadBoolean();
                scheme.WormPlacement = b.ReadBoolean();
                scheme.InitialWormEnergy = b.ReadByte();
                scheme.TurnTime = b.ReadSByte();
                scheme.RoundTime = b.ReadSByte();
                scheme.NumberOfRounds = b.ReadByte();
                scheme.Blood = b.ReadBoolean();
                scheme.AquaSheep = b.ReadBoolean();
                scheme.SheepHeaven = b.ReadBoolean();
                scheme.GodWorms = b.ReadBoolean();
                scheme.IndestructibleLand = b.ReadBoolean();
                scheme.UpgradedGrenade = b.ReadBoolean();
                scheme.UpgradedShotgun = b.ReadBoolean();
                scheme.UpgradedClusterBombs = b.ReadBoolean();
                scheme.UpgradedLongbow = b.ReadBoolean();
                scheme.TeamWeapons = b.ReadBoolean();
                scheme.SuperWeapons = b.ReadBoolean();
                scheme.WeaponInfo = ReadWeapons(b);
            }
            return scheme;
        }

        private static IReadOnlyCollection<IReadOnlyWeapon> ReadWeapons(BinaryReader b)
        {
            var weapons = new List<IReadOnlyWeapon>();
            foreach (var weaponName in Weapons.AllWeapons)
            {
                var weapon = new Weapon(weaponName);
                if (b.BaseStream.Position <= b.BaseStream.Length)
                {
                    weapon.Ammo = b.ReadByte();
                    weapon.Power = b.ReadByte() + 1;
                    weapon.Delay = b.ReadByte();
                    weapon.CrateProbability = b.ReadByte();
                }
                weapons.Add(weapon);
            }
            return weapons;
        }
    }
}

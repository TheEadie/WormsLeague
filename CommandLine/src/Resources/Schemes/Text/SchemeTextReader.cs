using System;
using System.IO;
using Syroot.Worms.Armageddon;

namespace Worms.Resources.Schemes.Text
{
    public class SchemeTextReader : ISchemeTextReader
    {
        public Scheme GetModel(string definition)
        {
            var scheme = new Scheme();
            using var b = new StringReader(definition);

            scheme.Version = SchemeVersion.Version2;

            // Skip over some heading lines
            b.ReadLine();
            b.ReadLine();
            b.ReadLine();
            b.ReadLine();

            scheme.HotSeatTime = GetByte(b);
            scheme.RetreatTime = GetByte(b);
            scheme.RetreatTimeRope = GetByte(b);
            scheme.ShowRoundTime = GetBool(b);
            scheme.Replays = GetBool(b);
            scheme.FallDamage = GetByte(b);
            scheme.ArtilleryMode = GetBool(b);
            scheme.Stockpiling = GetEnum<Stockpiling>(b);
            scheme.WormSelect = GetEnum<WormSelect>(b);
            scheme.SuddenDeathEvent = GetEnum<SuddenDeathEvent>(b);
            scheme.WaterRiseRate = GetByte(b);
            scheme.WeaponCrateProb = GetSbyte(b);
            scheme.HealthCrateProb = GetSbyte(b);
            scheme.UtilityCrateProb = GetSbyte(b);
            scheme.HealthCrateEnergy = GetByte(b);
            scheme.DonorCards = GetBool(b);
            scheme.ObjectTypes = GetEnum<MapObjectType>(b);
            scheme.ObjectCount = GetByte(b);
            scheme.MineDelayRandom = GetBool(b);
            scheme.MineDelay = GetByte(b);
            scheme.DudMines = GetBool(b);
            scheme.ManualWormPlacement = GetBool(b);
            scheme.WormEnergy = GetByte(b);
            scheme.TurnTimeInfinite = GetBool(b);
            scheme.TurnTime = GetByte(b);
            scheme.RoundTimeMinutes = GetByte(b);
            scheme.RoundTimeSeconds = GetByte(b);
            scheme.NumberOfWins = GetByte(b);
            scheme.Blood = GetBool(b);
            scheme.AquaSheep = GetBool(b);
            scheme.SheepHeaven = GetBool(b);
            scheme.GodWorms = GetBool(b);
            scheme.IndiLand = GetBool(b);
            scheme.UpgradeGrenade = GetBool(b);
            scheme.UpgradeShotgun = GetBool(b);
            scheme.UpgradeCluster = GetBool(b);
            scheme.UpgradeLongbow = GetBool(b);
            scheme.TeamWeapons = GetBool(b);
            scheme.SuperWeapons = GetBool(b);

            // Skip over the middle heading
            b.ReadLine();
            b.ReadLine();
            b.ReadLine();
            b.ReadLine();
            b.ReadLine();
            b.ReadLine();
            b.ReadLine();

            foreach (var weaponName in (Weapon[])Enum.GetValues(typeof(Weapon)))
            {
                var (ammo, power, delay, prob) = GetWeaponDetails(b);
                scheme.Weapons[weaponName].Ammo = ammo;
                scheme.Weapons[weaponName].Power = power;
                scheme.Weapons[weaponName].Delay = delay;
                scheme.Weapons[weaponName].Prob = prob;
            }

            return scheme;
        }

        private static byte GetByte(TextReader b)
        {
            return (byte)GetInt(b);
        }

        private static sbyte GetSbyte(TextReader b)
        {
            return (sbyte)GetInt(b);
        }

        private static bool GetBool(TextReader b)
        {
            return bool.Parse(GetValue(b.ReadLine()));
        }

        private static int GetInt(TextReader b)
        {
            return int.Parse(GetValue(b.ReadLine()));
        }

        private static (sbyte, byte, sbyte, sbyte) GetWeaponDetails(TextReader b)
        {
            var line = b.ReadLine();
            var ammo = (sbyte)int.Parse(GetValue(line.Substring(0, 44)));
            var power = (byte)int.Parse(GetValue(line.Substring(44, 10)));
            var delay = (sbyte)int.Parse(GetValue(line.Substring(55, 20)));
            var prob = (sbyte)int.Parse(GetValue(line.Substring(75, line.Length - 75)));

            return (ammo, power, delay, prob);
        }

        private static T GetEnum<T>(TextReader b) where T : struct
        {
            return Enum.Parse<T>(GetValue(b.ReadLine()));
        }

        private static string GetValue(string text)
        {
            var startIndex = text.IndexOf('[') + 1;
            var endIndex = text.IndexOf(']');
            var substring = text.Substring(startIndex, endIndex - startIndex);
            return substring;
        }
    }
}

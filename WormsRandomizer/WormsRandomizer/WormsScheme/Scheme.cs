using System.Collections.Generic;
using System.Linq;

namespace WormsRandomizer.WormsScheme
{
    /// <summary>
    /// See http://worms2d.info/Game_scheme_file
    /// </summary>
    public class Scheme : IReadOnlyScheme
    {
        public string Signature { get; set; } = "SCHM";
        public int Version { get; set; } = 2;
        public int HotSeatDelay { get; set; } = 5;
        public int RetreatTime { get; set; } = 3;
        public int RopeRetreatTime { get; set; } = 5;
        public bool DisplayTotalRoundTime { get; set; } = true;
        public bool AutomaticReplays { get; set; } = false;
        public int FallDamage { get; set; } = 1;
        public bool ArtilleryMode { get; set; } = false;
        public byte StockpilingMode { get; set; } = 0;
        public byte WormSelect { get; set; } = 0;
        public byte SuddenDeathEvent { get; set; } = 3;
        public int WaterRiseRate { get; set; } = 2;
        public int WeaponCrateProbability { get; set; } = 100;
        public bool DonorCards { get; set; } = true;
        public int HealthCrateProbability { get; set; } = 0;
        public int HealthCrateEnergy { get; set; } = 50;
        public int UtilityCrateProbability { get; set; } = 0;
        public int HazardObjectTypes { get; set; } = 43;
        public int MineDelay { get; set; } = 3;
        public bool DudMines { get; set; } = false;
        public bool WormPlacement { get; set; } = false;
        public int InitialWormEnergy { get; set; } = 100;
        public int TurnTime { get; set; } = 45;
        public int RoundTime { get; set; } = 10;
        public int NumberOfRounds { get; set; } = 0;
        public bool Blood { get; set; } = false;
        public bool AquaSheep { get; set; } = false;
        public bool SheepHeaven { get; set; } = false;
        public bool GodWorms { get; set; } = false;
        public bool IndestructibleLand { get; set; } = false;
        public bool UpgradedGrenade { get; set; } = false;
        public bool UpgradedShotgun { get; set; } = false;
        public bool UpgradedClusterBombs { get; set; } = false;
        public bool UpgradedLongbow { get; set; } = false;
        public bool TeamWeapons { get; set; } = false;
        public bool SuperWeapons { get; set; } = true;

        public IReadOnlyCollection<IReadOnlyWeapon> WeaponInfo { get; set; } = Weapons.AllWeapons.Select(x => new Weapon(x)).ToArray();
    }
}

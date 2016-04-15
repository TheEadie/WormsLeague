using System.Collections.Generic;

namespace WormsScheme
{
    /// <summary>
    /// See http://worms2d.info/Game_scheme_file
    /// </summary>
    public class Scheme
    {
        public string Signature { get; }
        public int Version { get; }
        public int HotSeatDelay { get; }
        public int RetreatTime { get; }
        public int RopeRetreatTime { get; }
        public bool DisplayTotalRoundTime { get; }
        public bool AutomaticReplays { get; }
        public int FallDamage { get; }
        public bool ArtilleryMode { get; }
        public byte StockpilingMode { get; }
        public byte WormSelect { get; }
        public byte SuddenDeathEvent { get; }
        public int WaterRiseRate { get; }
        public int WeaponCrateProbability { get; }
        public bool DonorCards { get; }
        public int HealthCrateProbability { get; }
        public int HealthCrateEnergy { get; }
        public int UtilityCrateProbability { get; }
        public int HazardObjectTypes { get; }
        public int MineDelay { get; }
        public bool DudMines { get; }
        public bool WormPlacement { get; }
        public int InitialWormEnergy { get; }
        public int TurnTime { get; }
        public int RoundTime { get; }
        public int NumberOfRounds { get; }
        public bool Blood { get; }
        public bool AquaSheep { get; }
        public bool SheepHeaven { get; }
        public bool GodWorms { get; }
        public bool IndestructibleLand { get; }
        public bool UpgradedGrenade { get; }
        public bool UpgradedShotgun { get; }
        public bool UpgradedClusterBombs { get; }
        public bool UpgradedLongbow { get; }
        public bool TeamWeapons { get; }
        public bool SuperWeapons { get; }

        public IEnumerable<Weapon> Weapons { get; } 

        public Scheme(string signature, int version, int hotSeatDelay, int retreatTime, int ropeRetreatTime,
            bool displayTotalRoundTime, bool automaticReplays, int fallDamage, bool artilleryMode, byte stockpilingMode,
            byte wormSelect, byte suddenDeathEvent, int waterRiseRate, int weaponCrateProbability, bool donorCards,
            int healthCrateProbability, int healthCrateEnergy, int utilityCrateProbability, int hazardObjectTypes,
            int mineDelay, bool dudMines, bool wormPlacement, int initialWormEnergy, int turnTime, int roundTime,
            int numberOfRounds, bool blood, bool aquaSheep, bool sheepHeaven, bool godWorms, bool indestructibleLand, bool upgradedGrenade,
            bool upgradedShotgun, bool upgradedClusterBombs, bool upgradedLongbow, bool teamWeapons, bool superWeapons, IEnumerable<Weapon> weapons)
        {
            Signature = signature;
            Version = version;
            HotSeatDelay = hotSeatDelay;
            RetreatTime = retreatTime;
            RopeRetreatTime = ropeRetreatTime;
            DisplayTotalRoundTime = displayTotalRoundTime;
            AutomaticReplays = automaticReplays;
            FallDamage = fallDamage;
            ArtilleryMode = artilleryMode;
            StockpilingMode = stockpilingMode;
            WormSelect = wormSelect;
            SuddenDeathEvent = suddenDeathEvent;
            WaterRiseRate = waterRiseRate;
            WeaponCrateProbability = weaponCrateProbability;
            DonorCards = donorCards;
            HealthCrateProbability = healthCrateProbability;
            HealthCrateEnergy = healthCrateEnergy;
            UtilityCrateProbability = utilityCrateProbability;
            HazardObjectTypes = hazardObjectTypes;
            MineDelay = mineDelay;
            DudMines = dudMines;
            WormPlacement = wormPlacement;
            InitialWormEnergy = initialWormEnergy;
            TurnTime = turnTime;
            RoundTime = roundTime;
            NumberOfRounds = numberOfRounds;
            Blood = blood;
            AquaSheep = aquaSheep;
            GodWorms = godWorms;
            IndestructibleLand = indestructibleLand;
            UpgradedGrenade = upgradedGrenade;
            UpgradedShotgun = upgradedShotgun;
            UpgradedClusterBombs = upgradedClusterBombs;
            UpgradedLongbow = upgradedLongbow;
            TeamWeapons = teamWeapons;
            SuperWeapons = superWeapons;
            Weapons = weapons;
            SheepHeaven = sheepHeaven;
        }
    }
}

using System.Collections.Generic;

namespace WormsRandomizer.WormsScheme
{
    public interface IReadOnlyScheme
    {
        string Signature { get; }
        int Version { get; }
        int HotSeatDelay { get; }
        int RetreatTime { get; }
        int RopeRetreatTime { get; }
        bool DisplayTotalRoundTime { get; }
        bool AutomaticReplays { get; }
        int FallDamage { get; }
        bool ArtilleryMode { get; }
        byte StockpilingMode { get; }
        byte WormSelect { get; }
        byte SuddenDeathEvent { get; }
        int WaterRiseRate { get; }
        int WeaponCrateProbability { get; }
        bool DonorCards { get; }
        int HealthCrateProbability { get; }
        int HealthCrateEnergy { get; }
        int UtilityCrateProbability { get; }
        int HazardObjectTypes { get; }
        int MineDelay { get; }
        bool DudMines { get; }
        bool WormPlacement { get; }
        int InitialWormEnergy { get; }
        int TurnTime { get; }
        int RoundTime { get; }
        int NumberOfRounds { get; }
        bool Blood { get; }
        bool AquaSheep { get; }
        bool SheepHeaven { get; }
        bool GodWorms { get; }
        bool IndestructibleLand { get; }
        bool UpgradedGrenade { get;  }
        bool UpgradedShotgun { get; }
        bool UpgradedClusterBombs { get; }
        bool UpgradedLongbow { get; }
        bool TeamWeapons { get; }
        bool SuperWeapons { get; }
        IReadOnlyCollection<IReadOnlyWeapon> WeaponInfo { get; }
    }
}
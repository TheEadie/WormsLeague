namespace WormsRandomizer.Config
{
    internal class SchemeRandomizerConfig
    {
        public int WormHealth { get; set; } = 100;

        public bool AllowSuperWeapons { get; set; } = true;
        public bool SheepHeaven { get; set; } = false;
        public bool UpgradedGrenade { get; set; } = false;
        public bool UpgradedShotgun { get; set; } = false;
        public bool UpgradedClusterBombs { get; set; } = false;
        public bool UpgradedLongbow { get; set; } = false;
        public bool AquaSheep { get; set; } = false;

        public bool AllowDudMines { get; set; } = false;
        public bool RandomPerMine { get; set; } = false;
        public bool RandomFloodRate { get; set; } = false;

        public bool OutputJson { get; set; } = false;
        public bool OutputScheme { get; set; } = true;
        public bool OutputStarting { get; set; } = true;
        public bool OutputSummary { get; set; } = false;

        public WeaponRandomizerConfig WeaponRandomizerConfig { get; set; } = new WeaponRandomizerConfig();
    }
}
using System.Collections.Generic;

namespace WormsRandomizer.Config
{
    internal class WeaponRandomizerConfig
    {
        public bool PowerIsDelayed { get; set; } = true;
        public int NumPowerfulWeapons { get; set; } = 5;
        public int NumStartingWeapons { get; set; } = 4;
        public int UtilityStartingAmmo { get; set; } = 1;
        public int WeaponStartingAmmo { get; set; } = 1;

        public IList<string> RequiredStartingWeapons { get; set; } = new List<string>();
        public IList<string> PromotedWeapons { get; set; } = new List<string>();
        public IList<string> BannedWeapons { get; set; } = new List<string>();
    }
}

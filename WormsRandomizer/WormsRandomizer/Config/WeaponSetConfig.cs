using System.Collections.Generic;
using Newtonsoft.Json;

namespace WormsRandomizer.Config
{
    [JsonObject]
    internal class WeaponSetConfig
    {
        [JsonRequired]
        public IReadOnlyCollection<WeaponConfig> Weapons { get; set; }

        [JsonRequired]
        public IReadOnlyCollection<string> MovementWeapons { get; set; }

        [JsonRequired]
        public IReadOnlyCollection<string> UtilityWeapons { get; set; }

        [JsonRequired]
        public IReadOnlyCollection<string> SuperWeapons { get; set; }
    }
}
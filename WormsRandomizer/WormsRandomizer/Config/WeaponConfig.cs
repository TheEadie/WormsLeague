using System.Collections.Generic;
using Newtonsoft.Json;

namespace WormsRandomizer.Config
{
    [JsonObject]
    internal class WeaponConfig
    {
        [JsonRequired]
        public string Name { get; set; }

        [JsonRequired]
        public IReadOnlyList<int> Power { get; set; }
    }
}

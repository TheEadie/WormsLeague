using System.IO;
using Newtonsoft.Json;

namespace WormsRandomizer.Config
{
    internal class WeaponSetConfigReader : IWeaponSetConfigReader
    {
        public WeaponSetConfig ReadConfig()
        {
            var text = File.ReadAllText("WeaponSetConfig.json");
            return JsonConvert.DeserializeObject<WeaponSetConfig>(text);
        }
    }
}

namespace WormsRandomizer.WormsScheme
{
    public class Weapon : IReadOnlyWeapon
    {
        public string Name { get; }
        public int Ammo { get; set; }
        public int Power { get; set; }
        public int Delay { get; set; }
        public int CrateProbability { get; set; }

        public Weapon(string name)
        {
            Name = name;
        }

        public Weapon(string configName, int ammo, int power, int delay, int crateChance)
        {
            Name = configName;
            Ammo = ammo;
            Power = power;
            Delay = delay;
            CrateProbability = crateChance;
        }
    }
}

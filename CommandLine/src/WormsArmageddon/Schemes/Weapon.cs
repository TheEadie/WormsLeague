namespace Worms.WormsArmageddon.Schemes
{
    public class Weapon
    {
        public string Name { get; }
        public int Ammo { get; }
        public int Power { get; }
        public int Delay { get; }
        public int CrateProbability { get; }

        public Weapon(string name, int ammo, int power, int delay, int crateProbability)
        {
            Name = name;
            Ammo = ammo;
            Power = power;
            Delay = delay;
            CrateProbability = crateProbability;
        }
    }
}

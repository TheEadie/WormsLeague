namespace WormsRandomizer.WormsScheme
{
    public interface IReadOnlyWeapon
    {
        string Name { get; }
        int Ammo { get; }
        int Power { get; }
        int Delay { get; }
        int CrateProbability { get; }
    }
}
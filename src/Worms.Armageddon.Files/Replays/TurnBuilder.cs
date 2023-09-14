namespace Worms.Armageddon.Files.Replays;

internal class TurnBuilder
{
    private Team _team;
    private TimeSpan _start;
    private TimeSpan _end;
    private readonly HashSet<Weapon> _weapons = new();
    private readonly List<Damage> _damage = new();

    public TurnBuilder WithStartTime(TimeSpan start)
    {
        _start = start;
        return this;
    }

    public TurnBuilder WithEndTime(TimeSpan end)
    {
        _end = end;
        return this;
    }

    public TurnBuilder WithTeam(Team team)
    {
        _team = team;
        return this;
    }

    public TurnBuilder WithWeapon(Weapon weapon)
    {
        _ = _weapons.Add(weapon);
        return this;
    }

    public TurnBuilder WithDamage(Damage damage)
    {
        _damage.Add(damage);
        return this;
    }

    public Turn Build() => new(_start, _end, _team, _weapons, _damage);

    public bool HasRequiredDetails() => _end != default;
}

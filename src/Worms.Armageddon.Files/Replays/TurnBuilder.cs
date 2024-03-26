namespace Worms.Armageddon.Files.Replays;

internal sealed class TurnBuilder
{
    private Team? _team;
    private TimeSpan? _start;
    private TimeSpan? _end;
    private readonly HashSet<Weapon> _weapons = [];
    private readonly List<Damage> _damage = [];

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

    public Turn Build() =>
        HasRequiredDetails()
            ? new Turn(_start!.Value, _end!.Value, _team!, _weapons, _damage)
            : throw new InvalidOperationException("Missing required details to create a turn");

    public bool HasRequiredDetails() => _team != null && _start != null && _end != null;
}

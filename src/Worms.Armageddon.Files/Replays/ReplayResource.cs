namespace Worms.Armageddon.Files.Replays;

public record ReplayResource(
    DateTime Date,
    bool Processed,
    IReadOnlyCollection<Team> Teams,
    string Winner,
    IReadOnlyCollection<Turn> Turns,
    string FullLog);

public record Team(string Name, string Machine, TeamColour Colour)
{
    public static Team Local(string name, TeamColour colour) => new(name, "local", colour);

    public static Team Remote(string name, string machine, TeamColour colour) => new(name, machine, colour);
};

public enum TeamColour
{
    Red,
    Blue,
    Green,
    Yellow,
    Magenta,
    Cyan
}

public record Turn(
    TimeSpan Start,
    TimeSpan End,
    Team Team,
    IReadOnlyCollection<Weapon> Weapons,
    IReadOnlyCollection<Damage> Damage);

public record Weapon(string Name, uint? Fuse, string? Modifier);

public record Damage(Team Team, uint HealthLost, uint WormsKilled);

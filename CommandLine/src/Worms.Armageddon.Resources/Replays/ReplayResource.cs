using System;
using System.Collections.Generic;

namespace Worms.Armageddon.Resources.Replays
{
    public record ReplayResource(
        DateTime Date,
        string Context,
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

    public record Turn(TimeSpan Start, TimeSpan End, Team Team, List<Weapon> Weapons, List<Damage> Damage);

    public record Weapon(string Name, int? Fuse, string Modifier);

    public record Damage(Team Team, int HealthLost, int WormsKilled);
}

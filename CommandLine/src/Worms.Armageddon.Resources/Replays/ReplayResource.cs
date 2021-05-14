using System;
using System.Collections.Generic;

namespace Worms.Armageddon.Resources.Replays
{
    public record ReplayResource(
        DateTime Date,
        string Context,
        bool Processed,
        List<Team> Teams,
        string Winner,
        List<Turn> Turns,
        string FullLog);

    public record Team(string Name, string Machine, string Colour);

    public record Turn(TimeSpan Start, TimeSpan End, Team Team, List<Weapon> Weapons, List<Damage> Damage);

    public record Weapon(string Name, int? Fuse, string Modifier);

    public record Damage(Team Team, int HealthLost, int WormsKilled);
}

using System;
using System.Collections.Generic;

namespace Worms.Armageddon.Resources.Replays
{
    internal class TurnBuilder
    {
        private Team _team;
        private TimeSpan _start;
        private TimeSpan _end;

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

        public Turn Build() => new(_start, _end, _team, new List<Weapon>(), new List<Damage>());
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Worms.Armageddon.Files.Replays
{
    internal class ReplayResourceBuilder
    {
        private DateTime _start;
        private readonly List<Team> _teams = new();
        private readonly List<Turn> _turns = new();
        private string _winner;
        private string _fullLog;

        public TurnBuilder CurrentTurn { get; private set; } = new();

        public ReplayResourceBuilder WithStartTime(DateTime start)
        {
            _start = start;
            return this;
        }

        public ReplayResourceBuilder WithTeam(Team team)
        {
            _teams.Add(team);
            return this;
        }

        public ReplayResourceBuilder FinaliseCurrentTurn()
        {
            if (CurrentTurn.HasRequiredDetails())
            {
                _turns.Add(CurrentTurn.Build());
                CurrentTurn = new TurnBuilder();
            }

            return this;
        }

        public ReplayResourceBuilder WithWinner(string winner)
        {
            _winner = winner;
            return this;
        }

        public ReplayResourceBuilder WithFullLog(string fullLog)
        {
            _fullLog = fullLog;
            return this;
        }

        public Team GetTeamByName(string name) => _teams.Single(x => x.Name == name);

        public ReplayResource Build() => new(_start, true, _teams, _winner, _turns, _fullLog);
    }
}

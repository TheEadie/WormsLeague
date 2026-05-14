namespace Worms.Armageddon.Files.Replays;

internal static class PlacementCalculator
{
    public static IReadOnlyCollection<Placement> Calculate(
        IReadOnlyCollection<Turn> turns,
        IReadOnlyCollection<Team> teams,
        string winner)
    {
        if (string.IsNullOrEmpty(winner))
        {
            return Array.Empty<Placement>();
        }

        // Pass 1: uncapped totals to find wormsPerTeam
        var uncappedTotals = new Dictionary<Team, uint>();
        foreach (var turn in turns)
        {
            foreach (var damage in turn.Damage)
            {
                uncappedTotals.TryGetValue(damage.Team, out var current);
                uncappedTotals[damage.Team] = current + damage.WormsKilled;
            }
        }

        var wormsPerTeam = uncappedTotals.Values.Count > 0 ? uncappedTotals.Values.Max() : 0u;

        if (wormsPerTeam == 0)
        {
            return Array.Empty<Placement>();
        }

        // Pass 2: capped pass to find elimination turn index per team
        var cumulativeKills = new Dictionary<Team, uint>();
        var eliminationTurn = new Dictionary<Team, int>();

        foreach (var (i, turn) in turns.Select((turn, i) => (i, turn)))
        {
            foreach (var damage in turn.Damage)
            {
                if (damage.WormsKilled == 0)
                {
                    continue;
                }

                cumulativeKills.TryGetValue(damage.Team, out var kills);
                if (kills >= wormsPerTeam)
                {
                    continue;
                }

                kills += damage.WormsKilled;
                cumulativeKills[damage.Team] = kills;

                if (kills >= wormsPerTeam && !eliminationTurn.ContainsKey(damage.Team))
                {
                    eliminationTurn[damage.Team] = i;
                }
            }
        }

        // Pass 3: assign positions based on rank key
        var placements = new List<Placement>(teams.Count);
        foreach (var team in teams)
        {
            var rankKey = eliminationTurn.TryGetValue(team, out var elim) ? elim : int.MaxValue;
            var position = teams.Count(t =>
            {
                var otherRankKey = eliminationTurn.TryGetValue(t, out var otherElim) ? otherElim : int.MaxValue;
                return otherRankKey > rankKey;
            }) + 1;

            placements.Add(new Placement(team, position));
        }

        return placements;
    }
}

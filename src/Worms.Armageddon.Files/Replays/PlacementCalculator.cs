namespace Worms.Armageddon.Files.Replays;

internal static class PlacementCalculator
{
    public static IReadOnlyCollection<Placement> Calculate(
        IReadOnlyCollection<Team> teams,
        string winner)
    {
        if (string.IsNullOrEmpty(winner))
        {
            return teams.Select(t => new Placement(t, null)).ToList();
        }

        if (winner == "Draw" || teams.All(t => t.Name != winner))
        {
            return teams.Select(t => new Placement(t, 1)).ToList();
        }

        return teams.Select(t => new Placement(t, t.Name == winner ? 1 : 2)).ToList();
    }
}

using Worms.Hub.Gateway.Ratings;

namespace Worms.Hub.Gateway.Tests.Worker;

internal sealed class FakeRatingsCalculator : IRatingsCalculator
{
    private readonly List<string> _calculatedLeagues = [];

    public IReadOnlyList<string> CalculatedLeagues => _calculatedLeagues;

    public LeagueRatingsChange Result { get; set; } = new([], []);

    public Exception? ExceptionToThrow { get; set; }

    public LeagueRatingsChange Calculate(string leagueId)
    {
        _calculatedLeagues.Add(leagueId);
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Result;
    }

    public void CalculateForTeam(string machine, string teamName) =>
        throw new NotSupportedException("Not used by the replay-update processor.");
}

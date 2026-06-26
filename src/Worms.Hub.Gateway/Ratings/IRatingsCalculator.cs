namespace Worms.Hub.Gateway.Ratings;

internal interface IRatingsCalculator
{
    LeagueRatingsChange Calculate(string leagueId);
    void CalculateForTeam(string machine, string teamName);
}

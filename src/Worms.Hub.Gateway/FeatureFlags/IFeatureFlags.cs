namespace Worms.Hub.Gateway.FeatureFlags;

internal interface IFeatureFlags
{
    Task<bool> IsLeaguesEnabledAsync();
    Task<bool> IsReplayLeagueFieldsEnabledAsync();
}

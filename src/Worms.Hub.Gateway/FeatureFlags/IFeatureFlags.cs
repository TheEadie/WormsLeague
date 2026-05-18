namespace Worms.Hub.Gateway.FeatureFlags;

internal interface IFeatureFlags
{
    Task<bool> IsTeamsEnabledAsync();
    Task<bool> IsEloRatingsEnabledAsync();
}

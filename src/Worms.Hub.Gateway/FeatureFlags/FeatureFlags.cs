using Worms.Hub.Storage.Database;

namespace Worms.Hub.Gateway.FeatureFlags;

internal sealed class GatewayFeatureFlags(DatabaseSchemaVersion schemaVersion) : IFeatureFlags
{
    private static readonly Version LeaguesMinVersion = new(0, 3);
    private static readonly Version PlacementsMinVersion = new(0, 6);

    public async Task<bool> IsLeaguesEnabledAsync()
    {
        var current = await schemaVersion.GetCurrentVersionAsync();
        return current is not null && current >= LeaguesMinVersion;
    }

    public async Task<bool> IsPlacementsEnabledAsync()
    {
        var current = await schemaVersion.GetCurrentVersionAsync();
        return current is not null && current >= PlacementsMinVersion;
    }
}

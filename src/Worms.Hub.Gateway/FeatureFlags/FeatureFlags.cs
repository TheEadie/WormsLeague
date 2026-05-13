using Worms.Hub.Storage.Database;

namespace Worms.Hub.Gateway.FeatureFlags;

internal sealed class GatewayFeatureFlags(DatabaseSchemaVersion schemaVersion) : IFeatureFlags
{
    private static readonly Version LeaguesMinVersion = new(0, 3);

    public async Task<bool> IsLeaguesEnabledAsync()
    {
        var current = await schemaVersion.GetCurrentVersionAsync();
        return current is not null && current >= LeaguesMinVersion;
    }
}

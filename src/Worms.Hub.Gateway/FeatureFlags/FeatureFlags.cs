using Worms.Hub.Storage.Database;

namespace Worms.Hub.Gateway.FeatureFlags;

internal sealed class GatewayFeatureFlags(DatabaseSchemaVersion schemaVersion) : IFeatureFlags
{
    private static readonly Version TeamsMinVersion = new(0, 7);
    private static readonly Version EloRatingsMinVersion = new(0, 8);

    public async Task<bool> IsTeamsEnabledAsync()
    {
        var current = await schemaVersion.GetCurrentVersionAsync();
        return current is not null && current >= TeamsMinVersion;
    }

    public async Task<bool> IsEloRatingsEnabledAsync()
    {
        var current = await schemaVersion.GetCurrentVersionAsync();
        return current is not null && current >= EloRatingsMinVersion;
    }
}

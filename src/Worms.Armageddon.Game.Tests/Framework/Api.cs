using Worms.Armageddon.Game.Fake;

namespace Worms.Armageddon.Game.Tests.Framework;

internal static class Api
{
    public static IWormsArmageddon GetWormsArmageddon(ApiType apiType, FakeConfiguration configuration) =>
        apiType switch
        {
            ApiType.FakeDependencies => FakeDependencies.GetWormsArmageddonApi(configuration),
            ApiType.RealDependencies => RealDependencies.GetWormsArmageddonApi(),
            ApiType.FakeComponent => FakeComponent.GetWormsArmageddonApi(configuration),
            _ => throw new NotSupportedException($"Invalid API type: {nameof(apiType)}")
        };
}

internal enum ApiType
{
    FakeDependencies = 0,
    RealDependencies = 1,
    FakeComponent = 2
}

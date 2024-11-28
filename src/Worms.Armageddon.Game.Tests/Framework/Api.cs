namespace Worms.Armageddon.Game.Tests.Framework;

internal static class Api
{
    public static IWormsArmageddon GetWormsArmageddon(ApiType apiType, InstallationType installationType) =>
        apiType switch
        {
            ApiType.FakeDependencies => FakeDependencies.GetWormsArmageddonApi(installationType),
            ApiType.RealDependencies => RealDependencies.GetWormsArmageddonApi(),
            ApiType.FakeComponent => throw new NotImplementedException(),
            _ => throw new NotSupportedException($"Invalid API type: {nameof(apiType)}")
        };
}

internal enum ApiType
{
    FakeDependencies = 0,
    RealDependencies = 1,
    FakeComponent = 2
}

internal enum InstallationType
{
    NotInstalled = 0,
    Installed = 1
}

namespace Worms.Armageddon.Game.Tests.Framework;

internal static class A
{
    public static IWormsArmageddonBuilder WormsArmageddon(ApiType apiType) =>
        apiType switch
        {
            ApiType.FakeDependencies => new FakeDependenciesBuilder(),
            ApiType.RealDependencies => new RealDependenciesBuilder(),
            ApiType.FakeComponent => new FakeComponentBuilder(),
            _ => throw new NotSupportedException($"Invalid API type: {nameof(apiType)}")
        };
}

internal enum ApiType
{
    FakeDependencies = 0,
    RealDependencies = 1,
    FakeComponent = 2
}

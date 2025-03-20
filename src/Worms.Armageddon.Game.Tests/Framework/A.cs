namespace Worms.Armageddon.Game.Tests.Framework;

internal static class A
{
    public static IWormsArmageddonBuilder WormsArmageddon(ApiType apiType) =>
        apiType switch
        {
            ApiType.Component => new ComponentWithMockedDependenciesBuilder(),
            ApiType.FakeComponent => new FakeComponentBuilder(),
            _ => throw new NotSupportedException($"Invalid API type: {nameof(apiType)}")
        };
}

internal enum ApiType
{
    Component = 0,
    FakeComponent = 1
}

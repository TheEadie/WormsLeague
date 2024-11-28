using NUnit.Framework;

namespace Worms.Armageddon.Game.Tests.Framework;

internal sealed class FakeDependenciesAttribute() : TestFixtureAttribute(ApiType.FakeDependencies);

internal sealed class RealDependenciesAttribute : TestFixtureAttribute
{
    public RealDependenciesAttribute()
        : base(ApiType.RealDependencies)
    {
        Explicit = true;
        Reason = "Real dependencies need setting up";
    }
}

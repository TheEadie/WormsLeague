using NUnit.Framework;

namespace Worms.Armageddon.Game.Tests.Framework;

internal sealed class ComponentAttribute() : TestFixtureAttribute(ApiType.Component);

internal sealed class FakeComponentAttribute() : TestFixtureAttribute(ApiType.FakeComponent);

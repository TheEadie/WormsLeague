namespace Worms.Armageddon.Game.Tests.Framework;

internal interface IWormsArmageddonBuilder
{
    IWormsArmageddonBuilder Installed(string? path = null, Version? version = null);
    IWormsArmageddonBuilder NotInstalled();
    IWormsArmageddon Build();
}

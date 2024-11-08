namespace Worms.Armageddon.Game;

internal interface IWormsRunner
{
    Task RunWorms(params string[] wormsArgs);
}

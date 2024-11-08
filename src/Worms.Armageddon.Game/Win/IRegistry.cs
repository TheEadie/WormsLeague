namespace Worms.Armageddon.Game.Win;

internal interface IRegistry
{
    string? GetValue(string keyName, string valueName, string? defaultValue);
}

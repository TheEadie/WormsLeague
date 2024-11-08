using System.Runtime.Versioning;

namespace Worms.Armageddon.Game.Win;

[SupportedOSPlatform("windows")]
internal class Registry : IRegistry
{
    public string GetValue(string keyName, string valueName, string? defaultValue) =>
        Microsoft.Win32.Registry.GetValue(keyName, valueName, defaultValue)?.ToString() ?? string.Empty;
}

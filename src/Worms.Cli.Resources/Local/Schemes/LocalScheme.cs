using Syroot.Worms.Armageddon;

namespace Worms.Cli.Resources.Local.Schemes;

public record LocalScheme(string Path, string Name, Scheme Details) : SchemeWithContext("context");

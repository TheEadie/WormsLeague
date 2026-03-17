using JetBrains.Annotations;
using Syroot.Worms.Armageddon;

namespace Worms.Cli.Resources.Local.Schemes;

[PublicAPI]
public record LocalScheme(string Path, string Name, Scheme Details) : SchemeWithContext("context");

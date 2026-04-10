using JetBrains.Annotations;

namespace Worms.Cli.Resources.Local.Schemes;

[PublicAPI]
public record LocalSchemeCreateParameters(string Name, string Folder, bool Random, string? Definition);

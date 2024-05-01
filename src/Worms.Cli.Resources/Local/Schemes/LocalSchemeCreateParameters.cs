namespace Worms.Cli.Resources.Local.Schemes;

public record LocalSchemeCreateParameters(string Name, string Folder, bool Random, string? Definition);

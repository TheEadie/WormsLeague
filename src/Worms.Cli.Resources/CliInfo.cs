namespace Worms.Cli.Resources;

public sealed record CliInfo(Version Version, string Folder, string FileName)
{
    public override string ToString() =>
        "Worms CLI: {" + $"Version: {Version.ToString(3)}, Folder: {Folder}, FileName: {FileName}" + "}";
}

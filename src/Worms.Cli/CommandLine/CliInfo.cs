namespace Worms.Cli.CommandLine;

internal sealed record CliInfo(Version Version, string Path)
{
    public override string ToString() => "Worms CLI: {" + $"Version: {Version}, " + $"Path: {Path}" + "}";
}

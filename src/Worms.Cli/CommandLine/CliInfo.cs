using System;

namespace Worms.Cli.CommandLine
{
    internal record CliInfo(Version Version, string Path)
    {
        public override string ToString() => "Worms CLI: {" + $"Version: {Version}, " + $"Path: {Path}" + "}";
    }
}

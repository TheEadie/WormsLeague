using System;

namespace Worms.Cli
{
    internal record CliInfo(Version Version, string Path)
    {
        public override string ToString() => "Worms CLI: {" + $"Version: {Version}, " + $"Path: {Path}" + "}";
    }
}

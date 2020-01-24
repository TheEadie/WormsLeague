using System;

namespace Worms.Cli
{
    internal class CliInfo
    {
        public Version Version { get; }
        public string Path { get; }

        public CliInfo(Version version, string path)
        {
            Version = version;
            Path = path;
        }

        public override string ToString()
        {
            return "Worms CLI: {" +
                   $"Version: {Version}, " +
                   $"Path: {Path}" +
                   "}";
        }
    }
}
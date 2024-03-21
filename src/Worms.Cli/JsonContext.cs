using System.Text.Json.Serialization;
using Worms.Cli.Configuration;

namespace Worms.Cli;

[JsonSerializable(typeof(Config))]
internal sealed partial class JsonContext : JsonSerializerContext;

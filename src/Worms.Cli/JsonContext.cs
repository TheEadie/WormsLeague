using System.Text.Json.Serialization;
using Worms.Cli.Configuration;
using Worms.Cli.Slack;

namespace Worms.Cli;

[JsonSerializable(typeof(SlackMessage))]
[JsonSerializable(typeof(Config))]
internal sealed partial class JsonContext : JsonSerializerContext { }

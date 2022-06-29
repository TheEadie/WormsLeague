using System.Text.Json.Serialization;

namespace Worms.Cli.Resources.Remote.Games;

public record RemoteGame(string Id, string Status, string HostMachine);
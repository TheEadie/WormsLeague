using System.Text.Json.Serialization;
using Worms.Cli.Resources.Remote;
using Worms.Cli.Resources.Remote.Auth;
using Worms.Cli.Resources.Remote.Auth.Responses;

namespace Worms.Cli.Resources;

[JsonSerializable(typeof(DeviceAuthorizationResponse))]
[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(AccessTokens))]
[JsonSerializable(typeof(LatestCliDtoV1))]
[JsonSerializable(typeof(CreateGameDtoV1))]
[JsonSerializable(typeof(GamesDtoV1))]
[JsonSerializable(typeof(IReadOnlyCollection<GamesDtoV1>))]
[JsonSerializable(typeof(CreateReplayDtoV1))]
[JsonSerializable(typeof(ReplayDtoV1))]
internal sealed partial class JsonContext : JsonSerializerContext;

using System.Text.Json.Serialization;

namespace Worms.Cli.Resources.Remote.Auth.Responses;

internal sealed record TokenResponse(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken,
    [property: JsonPropertyName("id_token")]
    string IdToken,
    [property: JsonPropertyName("token_type")]
    string TokenType,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn);

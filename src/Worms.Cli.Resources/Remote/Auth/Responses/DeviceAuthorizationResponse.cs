using System.Text.Json.Serialization;

namespace Worms.Cli.Resources.Remote.Auth.Responses;

internal sealed record DeviceAuthorizationResponse(
    [property: JsonPropertyName("device_code")]
    string DeviceCode,
    [property: JsonPropertyName("user_code")]
    string UserCode,
    [property: JsonPropertyName("verification_uri")]
    Uri VerificationUri,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,
    [property: JsonPropertyName("interval")]
    int Interval,
    [property: JsonPropertyName("verification_uri_complete")]
    Uri VerificationUriComplete);

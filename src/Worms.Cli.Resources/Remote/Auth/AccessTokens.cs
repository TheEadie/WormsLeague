using JetBrains.Annotations;

namespace Worms.Cli.Resources.Remote.Auth;

[PublicAPI]
public record AccessTokens(string? AccessToken, string? RefreshToken);

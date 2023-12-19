namespace Worms.Cli.Resources.Remote.Auth;

internal interface IAccessTokenRefreshService
{
    Task<AccessTokens> RefreshAccessTokens(AccessTokens current);
}

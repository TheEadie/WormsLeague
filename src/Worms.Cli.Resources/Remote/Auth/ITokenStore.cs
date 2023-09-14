namespace Worms.Cli.Resources.Remote.Auth;

public interface ITokenStore
{
    AccessTokens GetAccessTokens();
    void StoreAccessTokens(AccessTokens accessTokens);
}
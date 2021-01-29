namespace Worms.Server.Auth
{
    public interface ITokenStore
    {
        AccessTokens GetAccessTokens();
        void StoreAccessTokens(AccessTokens accessTokens);
    }
}
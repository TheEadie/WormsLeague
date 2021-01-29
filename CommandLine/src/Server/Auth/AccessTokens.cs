namespace Worms.Server.Auth
{
    public class AccessTokens
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }

        public AccessTokens(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}

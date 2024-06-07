using System.IdentityModel.Tokens.Jwt;

namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class UserDetailsService(ITokenStore tokenStore) : IUserDetailsService
{
    public bool IsUserLoggedIn()
    {
        var accessTokens = tokenStore.GetAccessTokens();
        return accessTokens.AccessToken is not null && accessTokens.RefreshToken is not null;
    }

    public string GetAnonymisedUserId()
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenStore.GetAccessTokens().AccessToken);
        return jwt.Claims.First(claim => claim.Type == "sub").Value;
    }
}

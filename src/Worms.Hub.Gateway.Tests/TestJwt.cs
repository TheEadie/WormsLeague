using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Worms.Hub.Gateway.Tests;

internal static class TestJwt
{
    internal const string Issuer = "https://test-issuer.local/";
    internal const string Audience = "worms.davideadie.dev";

    // The same key used by GatewayTestHost for validation
    internal static readonly SymmetricSecurityKey SigningKey = new(
        "test-signing-key-at-least-32-bytes-long!!"u8.ToArray());

    private static readonly SigningCredentials Credentials =
        new(SigningKey, SecurityAlgorithms.HmacSha256);

    private static readonly JsonWebTokenHandler Handler = new();

    private const string NameIdentifierClaimType =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

    /// <summary>
    /// Mint a token with the 'access' role and a controllable display-name ladder:
    /// optional subject (nameidentifier), nickname, and name claims. A null subject omits
    /// the nameidentifier claim entirely so the subject-absent rungs can be exercised.
    /// </summary>
    internal static string WithAccessRole(
        string? subject = "test-user",
        string? nickname = null,
        string? name = null)
    {
        var claims = new List<Claim> { new("permissions", "access") };
        if (subject is not null)
        {
            claims.Add(new Claim(NameIdentifierClaimType, subject));
        }

        if (nickname is not null)
        {
            claims.Add(new Claim("nickname", nickname));
        }

        if (name is not null)
        {
            claims.Add(new Claim("name", name));
        }

        return Mint(claims);
    }

    /// <summary>Mint a valid token that has no 'access' role — hits the 403 path.</summary>
    internal static string WithoutAccessRole() =>
        Mint([new Claim(NameIdentifierClaimType, "test-user"), new Claim("permissions", "other")]);

    private static string Mint(IEnumerable<Claim> claims)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = Credentials,
            Subject = new ClaimsIdentity(claims)
        };
        return Handler.CreateToken(descriptor);
    }
}

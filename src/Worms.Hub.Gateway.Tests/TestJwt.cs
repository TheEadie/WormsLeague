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

    /// <summary>Mint a token with the 'access' role — passes the [Authorize(Roles="access")] gate.</summary>
    internal static string WithAccessRole() => Mint(new Claim("permissions", "access"));

    /// <summary>Mint a valid token that has no 'access' role — hits the 403 path.</summary>
    internal static string WithoutAccessRole() => Mint(new Claim("permissions", "other"));

    private static string Mint(params Claim[] extraClaims)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = Credentials,
            Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new(
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                        "test-user")
                }.Concat(extraClaims))
        };

        return Handler.CreateToken(descriptor);
    }
}

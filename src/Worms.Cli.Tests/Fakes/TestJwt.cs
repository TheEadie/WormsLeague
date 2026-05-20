using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Worms.Cli.Tests.Fakes;

internal static class TestJwt
{
    public static string Create(string sub) =>
        new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                issuer: "tests",
                audience: "tests",
                claims: [new Claim("sub", sub)],
                expires: DateTime.UtcNow.AddHours(1)));
}

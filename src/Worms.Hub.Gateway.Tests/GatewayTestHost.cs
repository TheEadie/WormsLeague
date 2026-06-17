using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Storage.Database;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Gateway.Tests;

internal sealed class GatewayTestHost : WebApplicationFactory<Program>
{
    internal IRepository<Game> GamesRepository { get; } = Substitute.For<IRepository<Game>>();

    internal IAnnouncer Announcer { get; } = Substitute.For<IAnnouncer>();

    public GatewayTestHost()
    {
        // Boot only the gateway (not the worker) so no hosted services are started
        // and no Azurite / queue infrastructure is needed.
        Environment.SetEnvironmentVariable("WORMS_HUB_DISTRIBUTED", "true");
        Environment.SetEnvironmentVariable("WORMS_HUB_GATEWAY", "true");
        Environment.SetEnvironmentVariable("WORMS_HUB_WORKER", null);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // ── Replace Auth0 JWT validation with a local test issuer ──────────
            // Production wires up Authority + MetadataAddress which triggers an OIDC
            // discovery call to Auth0. PostConfigure replaces that with a static
            // configuration so tests run with no network access.
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Authority = null;
                    options.MetadataAddress = string.Empty;
                    options.RequireHttpsMetadata = false;
                    options.Configuration = new OpenIdConnectConfiguration();

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = TestJwt.SigningKey,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = TestJwt.Issuer,
                        ValidateIssuer = true,
                        ValidAudience = TestJwt.Audience,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        // Mirror prod config from appsettings.json so real role mapping is exercised:
                        // Auth:PermissionsClaim = "permissions", Auth:NameClaim = nameidentifier URI
                        RoleClaimType = "permissions",
                        NameClaimType =
                            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                    };
                });

            // ── Fake the seams ────────────────────────────────────────────────
            services.RemoveAll<IRepository<Game>>();
            services.AddSingleton(GamesRepository);

            services.RemoveAll<IAnnouncer>();
            services.AddSingleton(Announcer);
        });
    }

    /// <summary>Create an <see cref="HttpClient"/> that sends <paramref name="bearerToken"/> on every request.</summary>
    internal HttpClient CreateClient(string bearerToken)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Environment.SetEnvironmentVariable("WORMS_HUB_DISTRIBUTED", null);
            Environment.SetEnvironmentVariable("WORMS_HUB_GATEWAY", null);
        }

        base.Dispose(disposing);
    }
}

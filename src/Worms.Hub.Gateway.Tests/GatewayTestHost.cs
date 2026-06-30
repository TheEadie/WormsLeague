using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Worms.Hub.Gateway.Announcers;
using Worms.Hub.Gateway.Ratings;
using Worms.Hub.Queues;
using Worms.Hub.Storage.Fake;

namespace Worms.Hub.Gateway.Tests;

internal sealed class GatewayTestHost : WebApplicationFactory<Program>
{
    private readonly string _tempReplayFolder =
        Path.Combine(Path.GetTempPath(), "worms-gateway-tests", Guid.NewGuid().ToString("N"));

    internal IAnnouncer Announcer { get; } = Substitute.For<IAnnouncer>();

    internal IMessageQueue<ReplayToProcessMessage> ReplayProcessorQueue { get; } =
        Substitute.For<IMessageQueue<ReplayToProcessMessage>>();

    internal IRatingsCalculator RatingsCalculator { get; } = Substitute.For<IRatingsCalculator>();

    internal FakeHubStorage Storage => Services.GetRequiredService<FakeHubStorage>();

    internal MockFileSystem FileSystem { get; } = new();

    internal string SchemesFolder { get; } =
        Path.Combine(Path.GetTempPath(), "worms-gateway-tests-schemes", Guid.NewGuid().ToString("N"));

    internal string CliFolder { get; } =
        Path.Combine(Path.GetTempPath(), "worms-gateway-tests-cli", Guid.NewGuid().ToString("N"));

    internal string GameFolder { get; } =
        Path.Combine(Path.GetTempPath(), "worms-gateway-tests-game", Guid.NewGuid().ToString("N"));

    public GatewayTestHost()
    {
        // Boot only the gateway (not the worker) so no hosted services are started
        // and no Azurite / queue infrastructure is needed.
        Environment.SetEnvironmentVariable("WORMS_HUB_DISTRIBUTED", "true");
        Environment.SetEnvironmentVariable("WORMS_HUB_GATEWAY", "true");
        Environment.SetEnvironmentVariable("WORMS_HUB_WORKER", null);
        Environment.SetEnvironmentVariable("WORMS_STORAGE__TEMPREPLAYFOLDER", _tempReplayFolder);
        FileSystem.AddDirectory(SchemesFolder);
        Environment.SetEnvironmentVariable("WORMS_STORAGE__SCHEMESFOLDER", SchemesFolder);
        FileSystem.AddDirectory(CliFolder);
        Environment.SetEnvironmentVariable("WORMS_STORAGE__CLIFOLDER", CliFolder);
        FileSystem.AddDirectory(GameFolder);
        Environment.SetEnvironmentVariable("WORMS_STORAGE__GAMEFOLDER", GameFolder);
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

            services.AddFakeHubStorageServices();

            services.RemoveAll<IFileSystem>();
            services.AddSingleton<IFileSystem>(FileSystem);

            services.RemoveAll<IAnnouncer>();
            services.AddSingleton(Announcer);

            services.RemoveAll<IMessageQueue<ReplayToProcessMessage>>();
            services.AddSingleton(ReplayProcessorQueue);

            services.RemoveAll<IRatingsCalculator>();
            services.AddSingleton(RatingsCalculator);
        });
    }

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
            Environment.SetEnvironmentVariable("WORMS_STORAGE__TEMPREPLAYFOLDER", null);
            Environment.SetEnvironmentVariable("WORMS_STORAGE__SCHEMESFOLDER", null);
            Environment.SetEnvironmentVariable("WORMS_STORAGE__CLIFOLDER", null);
            Environment.SetEnvironmentVariable("WORMS_STORAGE__GAMEFOLDER", null);
            TryDeleteFolder(_tempReplayFolder);
        }

        base.Dispose(disposing);
    }

    private static void TryDeleteFolder(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { /* best-effort cleanup */ }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using Serilog;

namespace Worms.Server.Auth
{
    public class LoginService : ILoginService
    {
        public async Task RequestLogin(ILogger logger, CancellationToken cancellationToken)
        {
            var options = new OidcClientOptions
            {
                Authority = "https://eadie.eu.auth0.com",
                ClientId = "0dBbKeIKO2UAzWfBh6LuGwWYSWGZPFHB",
                Scope = "openid profile offline_access",
                FilterClaims = false,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };

            var extraParameters = new Dictionary<string, string>
            {
                { "audience", "worms.davideadie.dev" }
            };

            var oidcClient = new OidcClient(options);
            var browser = new SystemBrowser(9005);

            oidcClient.Options.Browser = browser;
            oidcClient.Options.RedirectUri = browser.RedirectUri;

            var request = new LoginRequest()
            {
                FrontChannelExtraParameters = extraParameters   // To ensure access tokens are for a specific resource, and therefore come as jwt's
            };
            var result = await oidcClient.LoginAsync(request, cancellationToken);

            if (result.IsError)
            {
                logger.Error(result.Error);
            }
            else
            {
                logger.Verbose("Claims:");
                foreach (var claim in result.User.Claims)
                {
                    logger.Verbose($"{claim.Type}: {claim.Value}");
                }
                logger.Verbose("");

                logger.Verbose($"Identity token: {result.IdentityToken}");
                logger.Verbose($"Access token:   {result.AccessToken}");
                logger.Verbose($"Refresh token:  {result.RefreshToken ?? "none"}");
            }
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Worms.Gateway.Auth
{
    /// <summary>
    /// This authorisation handler will bypass all requirements
    /// </summary>
    public class AllowAnonymous : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToList())
            {
                context.Succeed(requirement); //Simply pass all requirements
            }

            return Task.CompletedTask;
        }
    }
}
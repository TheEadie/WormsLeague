using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Worms.Gateway.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        [HttpGet("~/login"), HttpPost("~/login")]
        public IActionResult LogIn()
        {
            return Ok();
        }

        [HttpGet("~/logout"), HttpPost("~/logout")]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync().ConfigureAwait(false);
            return Ok();
        }
    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Worms.Gateway.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("~/signout"), HttpPost("~/signout")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }
    }
}
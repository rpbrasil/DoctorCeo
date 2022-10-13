// *See https://github.com/aspnet-contrib/AspNet.Security.OAuth.Provider

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace DoctorCeo.Controllers;

public class AuthController : Controller
{
    
    [HttpGet("~/signin")]
    public async Task<IActionResult> SignIn() => View("SignIn", await DoctorCeo.Extensions.HttpContextExtensions.GetExternalProvidersAsync(HttpContext));

    // [EnableCors("AllowSpecificOrigins")]
    [HttpPost("~/signin")]
    public async Task<IActionResult> SignIn([FromForm] string provider)
    {
        if (!string.IsNullOrWhiteSpace(provider))
        {
            if (!await DoctorCeo.Extensions.HttpContextExtensions.IsProviderSupportedAsync(HttpContext, provider))
            {
                return BadRequest();
            }            
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, provider);
        }
        return BadRequest();
    }

    [HttpGet("~/signout")]
    [HttpPost("~/signout")]
    public IActionResult SignOutCurrentUser()
    {
        // Instruct the cookies middleware to delete the local cookie created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g Google or Facebook).
        return SignOut(new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
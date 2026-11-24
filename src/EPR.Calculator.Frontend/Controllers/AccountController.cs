using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

/// <summary>
/// Signs the user out and shows a signed-out page using the service layout.
/// Replaces Microsoft.Identity.Web.UI's sign-out, whose SignedOut Razor Page is
/// not reachable here (Razor Pages are not mapped) and so returned a 404.
/// </summary>
[AllowAnonymous]
public class AccountController : BaseController
{
    [HttpGet]
    [ActionName("SignOut")]
    public IActionResult SignOutUser()
    {
        HttpContext.Session.Clear();

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(SignedOut)),
        };

        return SignOut(
            properties,
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult SignedOut()
    {
        return View();
    }
}

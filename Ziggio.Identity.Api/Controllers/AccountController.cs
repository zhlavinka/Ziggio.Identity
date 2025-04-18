using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Ziggio.Identity.Api.ViewModels;
using Ziggio.Identity.Domain.Services;

namespace Ziggio.Identity.Api.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly ISignInService _signInService;
    private readonly IUserService _userService;

    public AccountController(
        ISignInService signInService,
        IUserService userService
    )
    {
        _signInService = signInService;
        _userService = userService;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;

        if (ModelState.IsValid)
        {
            var signInResult = await _signInService.PasswordSignInAsync(model.ApplicationId, model.Email, model.Password, true, false, cancellationToken);
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", SetValidationMessage(signInResult));
                return View(model);
            }

            var userResult = await _userService.GetUserByEmailAsync(model.ApplicationId, model.Email, cancellationToken);

            // returned to authorization endpoint via "challenge"
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, userResult.Data.UserId.ToString()),
                new Claim(ClaimTypes.Name, userResult.Data.Username),
                new Claim(ClaimTypes.Email, userResult.Data.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

            return Redirect(model.ReturnUrl);
        }

        return View(model);
    }

    private string SetValidationMessage(Domain.Models.SignInResult result)
    {
        if (result.IsNotAllowed)
        {
            return "User is not allowed to sign in";
        }
        else if (result.IsLockedOut)
        {
            return "User is locked out";
        }
        else if (!string.IsNullOrEmpty(result.Message))
        {
            return result.Message;
        }
        return "An unexpected error occurred while signing in";
    }
}
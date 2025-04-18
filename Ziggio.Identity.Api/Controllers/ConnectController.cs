using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Ziggio.Identity.Api.Extensions;
using Ziggio.Identity.Domain;
using Ziggio.Identity.Domain.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Ziggio.Identity.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IUserService _userService;

    public ConnectController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IUserService userService
    )
    {
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _userService = userService;
    }

    [HttpGet("authorize")]
    [HttpPost("authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AuthorizeAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
          ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved");

        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return Challenge(
              authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
              properties: new AuthenticationProperties
              {
                  RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                  Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
              });
        }

        var idClaim = result.Principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var nameClaim = result.Principal.Claims.First(c => c.Type == ClaimTypes.Name).Value;
        var emailClaim = result.Principal.Claims.First(c => c.Type == ClaimTypes.Email).Value;
        // todo - get rid of this hard-coded claim
        var roleClaim = Constants.Roles.Applications.Administrator;

        // create a new claims principal
        var claims = new List<Claim>
        {
            // 'subject' claim which is required
            new Claim(Claims.Subject, idClaim),
            new Claim(ClaimTypes.Name, nameClaim).SetDestinations(Destinations.AccessToken),
            new Claim(ClaimTypes.Email, emailClaim).SetDestinations(Destinations.AccessToken),
            new Claim(ClaimTypes.Role, roleClaim).SetDestinations(Destinations.AccessToken)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Set requested scopes (this is not done automatically)
        claimsPrincipal.SetScopes(request.GetScopes());

        // get and set resources
        var resources = await _scopeManager.ListResourcesAsync(claimsPrincipal.GetScopes()).ToListAsync();
        claimsPrincipal.SetResources(resources);

        // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public IActionResult Logout() => View();

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        // Ask ASP.NET Core Identity to delete the local and external cookies created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g Google or Facebook).
        //await _signInManager.SignOutAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application or to
        // the RedirectUri specified in the authentication properties if none was set.
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        );
    }

    [HttpPost("token")]
    [Produces("application/json")]
    public async Task<IActionResult> TokenAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
          ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved");

        ClaimsPrincipal claimsPrincipal;

        if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                ClaimTypes.Name,
                ClaimTypes.Role);

            // subject (sub) is a required field, we use the client id as the subject identifier here.
            identity.AddClaim(Claims.Subject, request.ClientId ?? throw new InvalidOperationException(), Destinations.AccessToken);

            claimsPrincipal = new ClaimsPrincipal(identity);

            var scopes = request.GetScopes();

            claimsPrincipal.SetScopes(scopes);

            var resources = await _scopeManager.ListResourcesAsync(scopes).ToListAsync();

            claimsPrincipal.SetResources(resources);
        }
        else if (request.IsAuthorizationCodeGrantType()
            || request.IsRefreshTokenGrantType())
        {
            // Let OpenIddict validate the incoming token (auth code or refresh token)
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            claimsPrincipal = result.Principal;
        }
        else
        {
            throw new NotImplementedException("The specified grant type is not implemented");
        }

        // triggers token issuance and refresh token rotation
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("userinfo")]
    public async Task<IActionResult> UserInfoAsync()
    {
        var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        var userInfo = new
        {
            Sub = claimsPrincipal.GetClaim(Claims.Subject),
            Name = claimsPrincipal.GetClaim(ClaimTypes.Name),
            Email = claimsPrincipal.GetClaim(ClaimTypes.Email),
            Role = claimsPrincipal.GetClaims(ClaimTypes.Role)
        };

        return Ok(userInfo);
    }
}

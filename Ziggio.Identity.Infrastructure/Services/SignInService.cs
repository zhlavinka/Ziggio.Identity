using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Services;
using Ziggio.Identity.Infrastructure.Extensions;

namespace Ziggio.Identity.Infrastructure.Services;

public class SignInService : ISignInService {
  private readonly IUserService _userService;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public SignInService(
    IUserService userService,
    IHttpContextAccessor httpContextAccessor) {
    _userService = userService;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure) {
    if (user is null) {
      throw new ArgumentNullException(nameof(user));
    }

    var error = await PreSignInCheckAsync(user);
    if (error is not null) {
      return error;
    }

    var result = await _userService.CheckPasswordAsync(user.ApplicationId, user.Username, password, CancellationToken.None);
    if (result.IsSuccessful()) {
      // reset lockout access fail count
      return SignInResult.Success;
    }

    // check access fail count
    // lockout if necessary

    return SignInResult.Failed("An error occurred while trying to login");
  }

  public Task<bool> IsLockedOutAsync(User user) {
    return Task.FromResult(false);
  }

  public async Task<SignInResult> PasswordSignInAsync(int applicationId, string username, string password, bool isPersistent, bool lockoutOnFailure, CancellationToken cancellationToken) {
    if (string.IsNullOrEmpty(username)) {
      throw new ArgumentNullException(nameof(username));
    }
    if (string.IsNullOrEmpty(password)) {
      throw new ArgumentNullException(nameof(password));
    }

    var findUserResult = await _userService.GetUserByEmailAsync(applicationId, username, cancellationToken);
    if (!findUserResult.IsSuccessful()) {
      return SignInResult.Failed("Invalid username");
    }

    var user = findUserResult.Data;
    var attempt = await CheckPasswordSignInAsync(user, password, lockoutOnFailure);

    if (!attempt.Succeeded) {
      return attempt;
    }

    await SignInAsync(user, new AuthenticationProperties {
      IsPersistent = isPersistent
    });

    return SignInResult.Success;
  }

  public async Task SignInAsync(User user, AuthenticationProperties properties) {
    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.Name, user.Username)
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    await _httpContextAccessor.HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));
  }

  public Task SignInOrTwoFactorAsync(User user, bool isPersistent, string? loginProvider = null, bool bypassTwoFactor = false) {
    throw new NotImplementedException();
  }

  public async Task SignOutAsync() {
    await _httpContextAccessor.HttpContext.SignOutAsync();
  }

  /// <summary>
  /// Checks if user is locked out
  /// Add other checks in this method
  /// </summary>
  /// <param name="user">The user to put through checks</param>
  /// <returns></returns>
  private async Task<SignInResult> PreSignInCheckAsync(User user) {
    if (await IsLockedOutAsync(user)) {
      return SignInResult.LockedOut;
    }
    return null;
  }
}
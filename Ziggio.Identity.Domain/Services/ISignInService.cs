using Microsoft.AspNetCore.Authentication;
using Ziggio.Identity.Domain.Models;

namespace Ziggio.Identity.Domain.Services;

public interface ISignInService {
  Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure);
  Task<SignInResult> PasswordSignInAsync(int applicationId, string username, string password, bool isPersistent, bool lockoutOnFailure, CancellationToken cancellationToken);
  Task SignInAsync(User user, AuthenticationProperties properties);
  Task SignOutAsync();
}

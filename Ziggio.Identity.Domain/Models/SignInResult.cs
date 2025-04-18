using Ziggio.Identity.Domain.Results;

namespace Ziggio.Identity.Domain.Models;

public class SignInResult {
  public bool Succeeded { get; protected set; }
  public bool IsLockedOut { get; protected set; }
  public bool IsNotAllowed { get; protected set; }
  public bool RequiresTwoFactor { get; protected set; }

  public string Message { get; protected set; } = "";

  public static SignInResult Success => new SignInResult { Succeeded = true };
  public static SignInResult Failed(string message) => new SignInResult { Message = message };
  public static SignInResult LockedOut => new SignInResult { IsLockedOut = true };
  public static SignInResult NotAllowed => new SignInResult { IsNotAllowed = true };

  public override string ToString() {
    return IsLockedOut ? "Lockedout" :
            IsNotAllowed ? "NotAllowed" :
              RequiresTwoFactor ? "RequiresTwoFactor" :
                Succeeded ? "Succeeded" : "Failed";
  }
}
using System.Security.Claims;

namespace Ziggio.Identity.Domain.Data.Entities;

public class User {
  public int UserId { get; set; }
  public int ApplicationId { get; set; }
  public string Username { get; set; }
  public string Email { get; set; }
  public bool EmailConfirmed { get; set; }
  public string PasswordHash { get; set; }
  public string PasswordSalt { get; set; }
  /// <summary>
  /// A random value that must change whenever a users credentials change
  /// </summary>
  public string SecurityStamp { get; set; }
  /// <summary>
  /// A random value that must change whenever a user is persisted to the store
  /// </summary>
  public string ConcurrencyStamp { get; set; }
  public string? PhoneNumber { get; set; } = null;
  public bool PhoneNumberConfirmed { get; set; }
  public bool TwoFactorEnabled { get; set; }
  public DateTimeOffset? LockoutEnd { get; set; }
  public bool LockoutEnabled { get; set; }
  public int AccessFailedCount { get; set; }

  public virtual List<UserRole> Roles { get; set; }
}
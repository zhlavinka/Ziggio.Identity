namespace Ziggio.Identity.Domain.Models;

public class User {
  public int UserId { get; set; }
  public int ApplicationId { get; set; }
  public string Username { get; set; }
  public string Email { get; set; }
  public bool EmailConfirmed { get; set; }
  public string PhoneNumber { get; set; }
  public bool PhoneNumberConfirmed { get; set; }
  public bool TwoFactorEnabled { get; set; }
  public DateTimeOffset? LockoutEnd { get; set; }
  public bool LockoutEnabled { get; set; }
  public int AccessFailedCount { get; set; }
  public List<UserRole> Roles { get; set; } = new List<UserRole>();
}
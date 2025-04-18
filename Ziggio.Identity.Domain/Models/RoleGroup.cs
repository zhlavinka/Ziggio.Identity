namespace Ziggio.Identity.Domain.Models;

public class RoleGroup {
  public int RoleGroupId { get; set; }
  public int ApplicationId { get; set; }
  public string Name { get; set; }

  public virtual List<Role> Roles { get; set; }
}
namespace Ziggio.Identity.Domain.Data.Entities;

public class Role {
  public int RoleId { get; set; }
  public int? RoleGroupId { get; set; }
  public string Name { get; set; }

  public virtual RoleGroup RoleGroup { get; set; }
  public virtual List<UserRole> Users { get; set; }
}
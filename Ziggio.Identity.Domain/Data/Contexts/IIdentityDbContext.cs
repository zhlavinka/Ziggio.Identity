using Microsoft.EntityFrameworkCore;
using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Domain.Data.Contexts;

public interface IIdentityDbContext {
  DbSet<RoleGroup> RoleGroups { get; set; }
  DbSet<Role> Roles { get; set; }
  DbSet<User> Users { get; set; }
  DbSet<UserRole> UserRoles { get; set; }
}
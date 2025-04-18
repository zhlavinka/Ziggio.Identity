using Microsoft.EntityFrameworkCore;
using Ziggio.Identity.Domain.Data.Contexts;
using Ziggio.Identity.Domain.Data.Entities;
using Ziggio.Identity.Infrastructure.Data.EntityTypeConfigurations;

namespace Ziggio.Identity.Infrastructure.Data.Contexts;

public class IdentityDbContext : DbContext, IIdentityDbContext {
  public DbSet<RoleGroup> RoleGroups { get; set; }
  public DbSet<Role> Roles { get; set; }
  public DbSet<User> Users { get; set; }
  public DbSet<UserRole> UserRoles { get; set; }

  public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : base(options) { }

  protected override void OnModelCreating(ModelBuilder builder) {
    builder.ApplyConfiguration(new RoleConfiguration());
    builder.ApplyConfiguration(new RoleGroupConfiguration());
    builder.ApplyConfiguration(new UserConfiguration());
    builder.ApplyConfiguration(new UserRoleConfiguration());
  }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Infrastructure.Data.EntityTypeConfigurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole> {
    public void Configure(EntityTypeBuilder<UserRole> builder) {
        builder.ToTable("UserRoles");
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne(ur => ur.User)
          .WithMany(u => u.Roles)
          .HasForeignKey(ur => ur.UserId);

        builder.HasOne(ur => ur.Role)
          .WithMany(r => r.Users)
          .HasForeignKey(ur => ur.RoleId);
    }
}

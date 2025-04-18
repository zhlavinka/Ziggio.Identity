using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Infrastructure.Data.EntityTypeConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role> {
  public void Configure(EntityTypeBuilder<Role> builder) {
    builder.HasKey(r => r.RoleId);
    builder.Property(r => r.RoleId).ValueGeneratedOnAdd();
    builder.Property(r => r.Name).IsRequired();

    builder.HasOne(r => r.RoleGroup)
      .WithMany(rg => rg.Roles)
      .HasForeignKey(r => r.RoleGroupId);

    builder.HasMany(r => r.Users)
      .WithOne(ur => ur.Role)
      .HasForeignKey(ur => ur.RoleId);
  }
}
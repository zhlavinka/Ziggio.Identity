using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Infrastructure.Data.EntityTypeConfigurations;

public class RoleGroupConfiguration : IEntityTypeConfiguration<RoleGroup> {
  public void Configure(EntityTypeBuilder<RoleGroup> builder) {
    builder.HasKey(rg => rg.RoleGroupId);

    builder.Property(rg => rg.RoleGroupId)
      .ValueGeneratedOnAdd();

    builder.Property(rg => rg.ApplicationId)
      .IsRequired();

    builder.Property(rg => rg.Name)
      .IsRequired();

    builder.HasIndex(rg => new { rg.ApplicationId, rg.Name })
      .IsUnique();

    builder.HasMany(rg => rg.Roles)
      .WithOne(r => r.RoleGroup)
      .HasForeignKey(r => r.RoleGroupId);
  }
}
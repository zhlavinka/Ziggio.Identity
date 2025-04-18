using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Infrastructure.Data.EntityTypeConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User> {
  public void Configure(EntityTypeBuilder<User> builder) {
    builder.HasKey(u => u.UserId);
    builder.Property(u => u.UserId).ValueGeneratedOnAdd();
    builder.Property(u => u.ApplicationId).IsRequired();
    builder.Property(u => u.Username).IsRequired();
    builder.Property(u => u.Email).HasDefaultValue("");
    builder.Property(u => u.EmailConfirmed).HasDefaultValue(false);
    builder.Property(u => u.PasswordHash);
    builder.Property(u => u.PasswordSalt);
    builder.Property(u => u.SecurityStamp);
    builder.Property(u => u.ConcurrencyStamp);
    builder.Property(u => u.PhoneNumber);
    builder.Property(u => u.PhoneNumberConfirmed).HasDefaultValue(false);
    builder.Property(u => u.TwoFactorEnabled).HasDefaultValue(false);
    builder.Property(u => u.LockoutEnd);
    builder.Property(u => u.LockoutEnabled).HasDefaultValue(false);
    builder.Property(u => u.AccessFailedCount).HasDefaultValue(0);

    builder.HasMany(u => u.Roles)
      .WithOne(ur => ur.User)
      .HasForeignKey(ur => ur.UserId);
  }
}
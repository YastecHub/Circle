using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512);

        builder.Property(user => user.FullName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(user => user.Phone)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(user => user.Branch)
            .HasMaxLength(100);

        builder.Property(user => user.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
    }
}

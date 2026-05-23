using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.TokenHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedByIp)
            .HasMaxLength(64);

        builder.Property(refreshToken => refreshToken.RevokedByIp)
            .HasMaxLength(64);

        builder.HasIndex(refreshToken => refreshToken.UserId);
        builder.HasIndex(refreshToken => refreshToken.TokenHash);
    }
}

using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class UserOtpConfiguration : IEntityTypeConfiguration<UserOtp>
{
    public void Configure(EntityTypeBuilder<UserOtp> builder)
    {
        builder.ToTable("user_otps");

        builder.HasKey(otp => otp.Id);

        builder.Property(otp => otp.CodeHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(otp => otp.Purpose)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(otp => new { otp.UserId, otp.Purpose, otp.ExpiresAtUtc });
    }
}

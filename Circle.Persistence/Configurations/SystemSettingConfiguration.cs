using Circle.Application.Common.Constants;
using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    private static readonly DateTime SeededAtUtc = new(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("system_settings");

        builder.HasKey(setting => setting.Id);

        builder.Property(setting => setting.Key)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(setting => setting.Key)
            .IsUnique();

        builder.Property(setting => setting.Value)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(setting => setting.Description)
            .HasMaxLength(500);

        builder.HasData(
            new
            {
                Id = Guid.Parse("31ba8b6d-b37a-4dc1-8247-6f1bc8f8e7b1"),
                Key = SystemSettingKeys.PaymentGateway,
                Value = "Monnify",
                Description = "Selected MVP payment gateway.",
                CreatedAtUtc = SeededAtUtc,
                UpdatedAtUtc = (DateTime?)null
            },
            new
            {
                Id = Guid.Parse("f0ce80ac-6d1d-4e5e-8b4e-2f3f0a57de68"),
                Key = SystemSettingKeys.InterestCalculationMethod,
                Value = "FlatRate",
                Description = "Selected MVP interest calculation method.",
                CreatedAtUtc = SeededAtUtc,
                UpdatedAtUtc = (DateTime?)null
            },
            new
            {
                Id = Guid.Parse("bed10e4a-9bb1-4939-b8e0-a6c432e4f37d"),
                Key = SystemSettingKeys.AutoApprovalThreshold,
                Value = "20000",
                Description = "Loans above this amount require Business Owner sign-off.",
                CreatedAtUtc = SeededAtUtc,
                UpdatedAtUtc = (DateTime?)null
            },
            new
            {
                Id = Guid.Parse("c9d69918-1202-42f8-9b61-116f56f36d2b"),
                Key = SystemSettingKeys.RepaymentFrequencies,
                Value = "Daily,Weekly",
                Description = "Repayment frequencies supported in MVP.",
                CreatedAtUtc = SeededAtUtc,
                UpdatedAtUtc = (DateTime?)null
            });
    }
}

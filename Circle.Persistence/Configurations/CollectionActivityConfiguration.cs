using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class CollectionActivityConfiguration : IEntityTypeConfiguration<CollectionActivity>
{
    public void Configure(EntityTypeBuilder<CollectionActivity> builder)
    {
        builder.ToTable("collection_activities");

        builder.HasKey(activity => activity.Id);

        builder.Property(activity => activity.ActivityType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(activity => activity.Outcome)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(activity => activity.PromisedAmount)
            .HasPrecision(18, 2);

        builder.Property(activity => activity.Notes)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasIndex(activity => activity.CustomerId);
        builder.HasIndex(activity => activity.LoanId);
        builder.HasIndex(activity => activity.RepaymentScheduleId);
        builder.HasIndex(activity => activity.OfficerUserId);
        builder.HasIndex(activity => activity.CreatedAtUtc);
        builder.HasIndex(activity => activity.PromisedPaymentDate);
    }
}

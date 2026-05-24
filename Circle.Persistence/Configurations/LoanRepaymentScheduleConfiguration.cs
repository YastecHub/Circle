using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class LoanRepaymentScheduleConfiguration : IEntityTypeConfiguration<LoanRepaymentSchedule>
{
    public void Configure(EntityTypeBuilder<LoanRepaymentSchedule> builder)
    {
        builder.ToTable("loan_repayment_schedules");

        builder.HasKey(schedule => schedule.Id);

        builder.Property(schedule => schedule.PrincipalAmount)
            .HasPrecision(18, 2);

        builder.Property(schedule => schedule.InterestAmount)
            .HasPrecision(18, 2);

        builder.Property(schedule => schedule.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(schedule => schedule.AmountPaid)
            .HasPrecision(18, 2);

        builder.Property(schedule => schedule.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(schedule => schedule.LoanId);
        builder.HasIndex(schedule => schedule.DueDate);
        builder.HasIndex(schedule => new { schedule.LoanId, schedule.InstallmentNumber })
            .IsUnique();
    }
}

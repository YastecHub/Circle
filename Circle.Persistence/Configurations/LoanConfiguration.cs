using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");

        builder.HasKey(loan => loan.Id);

        builder.Property(loan => loan.LoanCode)
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(loan => loan.LoanCode)
            .IsUnique();

        builder.Property(loan => loan.PrincipalAmount)
            .HasPrecision(18, 2);

        builder.Property(loan => loan.InterestRatePercent)
            .HasPrecision(9, 4);

        builder.Property(loan => loan.InterestAmount)
            .HasPrecision(18, 2);

        builder.Property(loan => loan.TotalRepayable)
            .HasPrecision(18, 2);

        builder.Property(loan => loan.RepaymentFrequency)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(loan => loan.InterestCalculationMethod)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(loan => loan.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(loan => loan.ApprovalNote)
            .HasMaxLength(500);

        builder.Property(loan => loan.RejectionReason)
            .HasMaxLength(500);

        builder.Property(loan => loan.Purpose)
            .HasMaxLength(500);

        builder.HasIndex(loan => loan.CustomerId);
        builder.HasIndex(loan => loan.Status);
        builder.HasIndex(loan => loan.RequestedByUserId);
        builder.HasIndex(loan => loan.ApprovedByUserId);

        builder.HasMany(loan => loan.RepaymentSchedules)
            .WithOne()
            .HasForeignKey(repayment => repayment.LoanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(loan => loan.RepaymentSchedules)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

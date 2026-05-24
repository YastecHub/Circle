using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class RepaymentConfiguration : IEntityTypeConfiguration<Repayment>
{
    public void Configure(EntityTypeBuilder<Repayment> builder)
    {
        builder.ToTable("repayments");

        builder.HasKey(repayment => repayment.Id);

        builder.Property(repayment => repayment.Amount)
            .HasPrecision(18, 2);

        builder.Property(repayment => repayment.Method)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(repayment => repayment.Reference)
            .HasMaxLength(120);

        builder.Property(repayment => repayment.Notes)
            .HasMaxLength(500);

        builder.HasIndex(repayment => repayment.LoanId);
        builder.HasIndex(repayment => repayment.CustomerId);
        builder.HasIndex(repayment => repayment.PaymentDate);
        builder.HasIndex(repayment => repayment.RecordedByUserId);
        builder.HasIndex(repayment => repayment.Reference);
    }
}

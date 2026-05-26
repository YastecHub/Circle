using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class LoanDisbursementConfiguration : IEntityTypeConfiguration<LoanDisbursement>
{
    public void Configure(EntityTypeBuilder<LoanDisbursement> builder)
    {
        builder.ToTable("loan_disbursements");

        builder.HasKey(disbursement => disbursement.Id);

        builder.Property(disbursement => disbursement.Amount)
            .HasPrecision(18, 2);

        builder.Property(disbursement => disbursement.Method)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(disbursement => disbursement.Reference)
            .HasMaxLength(120);

        builder.Property(disbursement => disbursement.Notes)
            .HasMaxLength(500);

        builder.HasIndex(disbursement => disbursement.LoanId)
            .IsUnique();

        builder.HasIndex(disbursement => disbursement.CustomerId);
        builder.HasIndex(disbursement => disbursement.DisbursementDate);
        builder.HasIndex(disbursement => disbursement.DisbursedByUserId);
        builder.HasIndex(disbursement => disbursement.Reference);
        builder.HasIndex(disbursement => new { disbursement.CustomerId, disbursement.DisbursementDate });
    }
}

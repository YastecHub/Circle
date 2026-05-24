using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class InboundPaymentConfiguration : IEntityTypeConfiguration<InboundPayment>
{
    public void Configure(EntityTypeBuilder<InboundPayment> builder)
    {
        builder.ToTable("inbound_payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Provider)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(payment => payment.ProviderReference)
            .HasMaxLength(160);

        builder.Property(payment => payment.AccountNumber)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2);

        builder.Property(payment => payment.AppliedAmount)
            .HasPrecision(18, 2);

        builder.Property(payment => payment.UnappliedAmount)
            .HasPrecision(18, 2);

        builder.Property(payment => payment.PayerName)
            .HasMaxLength(160);

        builder.Property(payment => payment.Narration)
            .HasMaxLength(500);

        builder.Property(payment => payment.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(payment => payment.FailureReason)
            .HasMaxLength(500);

        builder.HasIndex(payment => payment.ProviderReference);
        builder.HasIndex(payment => payment.AccountNumber);
        builder.HasIndex(payment => payment.CustomerId);
        builder.HasIndex(payment => payment.LoanId);
        builder.HasIndex(payment => payment.RepaymentId);
        builder.HasIndex(payment => payment.Status);
        builder.HasIndex(payment => payment.PaidAtUtc);
    }
}

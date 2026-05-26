using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class CustomerVirtualAccountConfiguration : IEntityTypeConfiguration<CustomerVirtualAccount>
{
    public void Configure(EntityTypeBuilder<CustomerVirtualAccount> builder)
    {
        builder.ToTable("customer_virtual_accounts");

        builder.HasKey(account => account.Id);

        builder.Property(account => account.Provider)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(account => account.AccountName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(account => account.AccountNumber)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(account => account.BankName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(account => account.BankCode)
            .HasMaxLength(40);

        builder.Property(account => account.ProviderReference)
            .HasMaxLength(160);

        builder.Property(account => account.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(account => account.CustomerId);
        builder.HasIndex(account => account.AccountNumber);
        builder.HasIndex(account => account.ProviderReference);
        builder.HasIndex(account => account.Status);
        builder.HasIndex(account => new { account.Provider, account.AccountNumber, account.Status });
    }
}

using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.CustomerCode)
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(customer => customer.CustomerCode)
            .IsUnique();

        builder.Property(customer => customer.PassbookReference)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(customer => customer.FullName)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(customer => customer.Phone)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(customer => customer.AlternatePhone)
            .HasMaxLength(32);

        builder.Property(customer => customer.Address)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(customer => customer.City)
            .HasMaxLength(100);

        builder.Property(customer => customer.State)
            .HasMaxLength(100);

        builder.Property(customer => customer.GovernmentIdType)
            .HasMaxLength(80);

        builder.Property(customer => customer.GovernmentIdNumber)
            .HasMaxLength(100);

        builder.Property(customer => customer.Gender)
            .HasMaxLength(40);

        builder.Property(customer => customer.Occupation)
            .HasMaxLength(120);

        builder.Property(customer => customer.GuarantorName)
            .HasMaxLength(160);

        builder.Property(customer => customer.GuarantorPhone)
            .HasMaxLength(32);

        builder.Property(customer => customer.GuarantorRelationship)
            .HasMaxLength(80);

        builder.Property(customer => customer.GuarantorIdNumber)
            .HasMaxLength(100);

        builder.Property(customer => customer.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(customer => customer.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(customer => customer.AssignedCreditOfficerId);

        builder.HasMany(customer => customer.Documents)
            .WithOne()
            .HasForeignKey(document => document.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(customer => customer.Documents)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class CustomerDocumentConfiguration : IEntityTypeConfiguration<CustomerDocument>
{
    public void Configure(EntityTypeBuilder<CustomerDocument> builder)
    {
        builder.ToTable("customer_documents");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(document => document.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(document => document.StoragePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(document => document.ContentType)
            .HasMaxLength(100);

        builder.HasIndex(document => document.CustomerId);
        builder.HasIndex(document => document.UploadedByUserId);
    }
}

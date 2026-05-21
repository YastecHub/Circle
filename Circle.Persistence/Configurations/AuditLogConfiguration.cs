using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(auditLog => auditLog.Id);

        builder.Property(auditLog => auditLog.Action)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(auditLog => auditLog.AffectedRecord)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(auditLog => auditLog.OldValue)
            .HasMaxLength(4000);

        builder.Property(auditLog => auditLog.NewValue)
            .HasMaxLength(4000);

        builder.Property(auditLog => auditLog.IpAddress)
            .HasMaxLength(64);

        builder.Property(auditLog => auditLog.Reason)
            .HasMaxLength(1000);

        builder.HasIndex(auditLog => auditLog.ActorUserId);
        builder.HasIndex(auditLog => auditLog.CreatedAtUtc);
    }
}

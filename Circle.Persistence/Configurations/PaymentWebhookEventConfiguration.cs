using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Circle.Persistence.Configurations;

public sealed class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> builder)
    {
        builder.ToTable("payment_webhook_events");

        builder.HasKey(webhookEvent => webhookEvent.Id);

        builder.Property(webhookEvent => webhookEvent.Provider)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(webhookEvent => webhookEvent.EventType)
            .HasMaxLength(120);

        builder.Property(webhookEvent => webhookEvent.EventReference)
            .HasMaxLength(180);

        builder.Property(webhookEvent => webhookEvent.PayloadJson)
            .HasColumnType("longtext")
            .IsRequired();

        builder.Property(webhookEvent => webhookEvent.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(webhookEvent => webhookEvent.ProcessingMessage)
            .HasMaxLength(1000);

        builder.HasIndex(webhookEvent => webhookEvent.Provider);
        builder.HasIndex(webhookEvent => webhookEvent.EventReference);
        builder.HasIndex(webhookEvent => webhookEvent.Status);
        builder.HasIndex(webhookEvent => webhookEvent.CreatedAtUtc);
        builder.HasIndex(webhookEvent => webhookEvent.InboundPaymentId);
    }
}

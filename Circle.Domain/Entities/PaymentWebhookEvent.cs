using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class PaymentWebhookEvent : BaseEntity
{
    private PaymentWebhookEvent()
    {
    }

    public PaymentWebhookEvent(
        string provider,
        string? eventType,
        string? eventReference,
        string payloadJson,
        bool isVerified)
    {
        Provider = provider.Trim();
        EventType = string.IsNullOrWhiteSpace(eventType) ? null : eventType.Trim();
        EventReference = string.IsNullOrWhiteSpace(eventReference) ? null : eventReference.Trim();
        PayloadJson = payloadJson;
        IsVerified = isVerified;
        Status = PaymentWebhookStatus.Received;
    }

    public string Provider { get; private set; } = string.Empty;
    public string? EventType { get; private set; }
    public string? EventReference { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public bool IsVerified { get; private set; }
    public PaymentWebhookStatus Status { get; private set; }
    public Guid? InboundPaymentId { get; private set; }
    public string? ProcessingMessage { get; private set; }

    public void MarkProcessed(Guid inboundPaymentId, string? message = null)
    {
        InboundPaymentId = inboundPaymentId;
        Status = PaymentWebhookStatus.Processed;
        ProcessingMessage = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
    }

    public void MarkIgnored(string message)
    {
        Status = PaymentWebhookStatus.Ignored;
        ProcessingMessage = message.Trim();
    }

    public void MarkDuplicate(string message)
    {
        Status = PaymentWebhookStatus.Duplicate;
        ProcessingMessage = message.Trim();
    }

    public void MarkFailed(string message)
    {
        Status = PaymentWebhookStatus.Failed;
        ProcessingMessage = message.Trim();
    }
}

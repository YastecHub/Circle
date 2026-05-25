namespace Circle.Domain.Enums;

public enum PaymentWebhookStatus
{
    Received = 1,
    Processed = 2,
    Ignored = 3,
    Duplicate = 4,
    Failed = 5
}

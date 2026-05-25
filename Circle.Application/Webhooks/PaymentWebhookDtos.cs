using Circle.Application.Reconciliation;
using Circle.Domain.Enums;

namespace Circle.Application.Webhooks;

public sealed record PaymentWebhookResponse(
    Guid Id,
    string Provider,
    string? EventType,
    string? EventReference,
    bool IsVerified,
    PaymentWebhookStatus Status,
    Guid? InboundPaymentId,
    string? ProcessingMessage,
    InboundPaymentResponse? InboundPayment,
    DateTime CreatedAtUtc);

public sealed record PaymentWebhookListResponse(
    IReadOnlyCollection<PaymentWebhookResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

using Circle.Domain.Enums;

namespace Circle.Application.Reconciliation;

public sealed record RecordInboundPaymentRequest(
    string? ProviderReference,
    string AccountNumber,
    decimal Amount,
    DateTime PaidAtUtc,
    string? PayerName,
    string? Narration);

public sealed record InboundPaymentResponse(
    Guid Id,
    string Provider,
    string? ProviderReference,
    string AccountNumber,
    decimal Amount,
    decimal AppliedAmount,
    decimal UnappliedAmount,
    DateTime PaidAtUtc,
    string? PayerName,
    string? Narration,
    Guid? CustomerId,
    Guid? LoanId,
    Guid? RepaymentId,
    InboundPaymentStatus Status,
    string? FailureReason,
    DateTime CreatedAtUtc);

public sealed record InboundPaymentListResponse(
    IReadOnlyCollection<InboundPaymentResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

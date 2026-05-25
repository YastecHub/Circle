using Circle.Domain.Enums;

namespace Circle.Application.Repayments;

public sealed record RecordRepaymentRequest(
    Guid LoanId,
    decimal Amount,
    DateOnly PaymentDate,
    RepaymentMethod Method,
    string? Reference,
    string? Notes);

public sealed record RepaymentResponse(
    Guid Id,
    Guid LoanId,
    Guid CustomerId,
    decimal Amount,
    DateOnly PaymentDate,
    RepaymentMethod Method,
    string? Reference,
    Guid? RecordedByUserId,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record RepaymentListResponse(
    IReadOnlyCollection<RepaymentResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
